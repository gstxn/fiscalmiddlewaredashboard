using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Infrastructure.Messaging;
using FiscalMiddleware.Domain.Enums;
using FiscalMiddleware.Domain.Entities;

namespace FiscalMiddleware.Worker;

public class FiscalMessageConsumer : BackgroundService
{
    private readonly ILogger<FiscalMessageConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    
    private IConnection _connection;
    private IModel _channel;
    private string _queueName;
    private string _dlqName;

    public FiscalMessageConsumer(ILogger<FiscalMessageConsumer> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _queueName = _configuration["RabbitMQ:QueueName"] ?? "transacoes_fiscais_queue";
        _dlqName = _queueName + "_dlq";

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "fiscal_mq",
            Password = _configuration["RabbitMQ:Password"] ?? "fiscal_mq_pass",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Configuração DLQ Exchange / Queue
        _channel.ExchangeDeclare("dlq_exchange", ExchangeType.Direct);
        _channel.QueueDeclare(_dlqName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_dlqName, "dlq_exchange", routingKey: _dlqName);

        // Configuração Main Queue
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        _logger.LogInformation("FiscalMessageConsumer iniciado. Conectado ao RabbitMQ.");
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var idempotencyService = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();
            var fiscalClient = scope.ServiceProvider.GetRequiredService<IExternalFiscalClient>();
            var transacaoRepo = scope.ServiceProvider.GetRequiredService<ITransacaoRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<FiscalMiddleware.Infrastructure.Data.FiscalDbContext>();
            
            var body = ea.Body.ToArray();
            var messageStr = Encoding.UTF8.GetString(body);
            var transacaoMsg = JsonSerializer.Deserialize<TransacaoMessageDto>(messageStr);
            
            using var logScope = _logger.BeginScope("CorrelationId: {CorrelationId}, TransacaoId: {TransacaoId}", 
                transacaoMsg.CorrelationId, transacaoMsg.TransacaoId);
            
            try
            {
                // 1. Verificar Idempotência no Redis (TTL de 24 horas)
                var acquired = await idempotencyService.AcquireLockAsync(transacaoMsg.ChaveIdempotencia, TimeSpan.FromHours(24), stoppingToken);
                if (!acquired)
                {
                    _logger.LogWarning("Transação já processada (Duplicidade detectada pela chave {Chave}). Descartando.", transacaoMsg.ChaveIdempotencia);
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation("Iniciando envio ao sistema externo.");

                // 2. Chamar a API externa (O Polly já cuida de timeout, retries e circuit breaker)
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var (statusCode, errorDetail) = await fiscalClient.EnviarTransacaoAsync(transacaoMsg.PayloadOriginal, stoppingToken);
                watch.Stop();

                // 3. Resgatar a transação no banco
                var transacao = await transacaoRepo.ObterPorIdAsync(transacaoMsg.TransacaoId, stoppingToken);
                if (transacao == null)
                {
                    _logger.LogError("Transação não encontrada no banco. Isso indica falha grave de consistência.");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                // 4. Analisar Resultado
                var isSuccess = statusCode >= 200 && statusCode < 300;
                var novoStatus = isSuccess ? StatusTransacao.Sucesso : StatusTransacao.Falha;
                var motivoFalha = isSuccess ? MotivoFalha.Nenhum : DeterminarMotivo(statusCode, errorDetail);

                var historico = new HistoricoProcessamento(
                    transacao.Id, 
                    1, // Polly fez as tentativas, nós contabilizamos como 1 execução do pipeline inteiro.
                    novoStatus, 
                    motivoFalha, 
                    statusCode, 
                    errorDetail ?? string.Empty, 
                    watch.ElapsedMilliseconds);
                    
                dbContext.Historicos.Add(historico);

                // Atualizar o status da transação para refletir na Dashboard e no banco
                var propertyInfo = typeof(Transacao).GetProperty("Status");
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(transacao, novoStatus);
                }

                await transacaoRepo.AtualizarAsync(transacao, stoppingToken);
                await dbContext.SaveChangesAsync(stoppingToken);

                if (isSuccess)
                {
                    _logger.LogInformation("Transação aprovada no sistema externo.");
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                else
                {
                    _logger.LogWarning("Transação falhou com status HTTP {StatusCode}. Enviando para DLQ.", statusCode);
                    // Roteia para DLQ
                    EnviarParaDLQ(body, transacaoMsg, motivoFalha, errorDetail);
                    _channel.BasicAck(ea.DeliveryTag, multiple: false); // ACK na fila principal
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro catastrófico ao processar mensagem. Movendo para DLQ.");
                EnviarParaDLQ(body, transacaoMsg, MotivoFalha.ErroInesperado, ex.Message);
                _channel.BasicAck(ea.DeliveryTag, multiple: false); // ACK na main
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        await Task.CompletedTask;
    }

    private MotivoFalha DeterminarMotivo(int statusCode, string erro)
    {
        if (statusCode == 408) return MotivoFalha.TimeoutExterno;
        if (statusCode >= 400 && statusCode < 500) return MotivoFalha.ErroValidacaoExterna;
        if (statusCode == 500 && (erro?.Contains("Circuit") == true || erro?.Contains("Broken") == true)) return MotivoFalha.CircuitoAberto;
        return MotivoFalha.ErroInesperado;
    }

    private void EnviarParaDLQ(byte[] originalBody, TransacaoMessageDto dto, MotivoFalha motivo, string detalheErro)
    {
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.Headers = new System.Collections.Generic.Dictionary<string, object>
        {
            { "MotivoFalha", motivo.ToString() },
            { "DetalheErro", detalheErro ?? "" }
        };

        _channel.BasicPublish(exchange: "dlq_exchange", routingKey: _dlqName, basicProperties: properties, body: originalBody);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }
}
