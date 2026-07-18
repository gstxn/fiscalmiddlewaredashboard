using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FiscalMiddleware.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        
        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var userName = configuration["RabbitMQ:UserName"] ?? "fiscal_mq";
        var password = configuration["RabbitMQ:Password"] ?? "fiscal_mq_pass";
        _queueName = configuration["RabbitMQ:QueueName"] ?? "transacoes_fiscais_queue";

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        // For production, connection resilience/retries should be added here
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Ensure queue exists
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public Task PublicarTransacaoAsync(Transacao transacao, CancellationToken cancellationToken)
    {
        var messageDto = new TransacaoMessageDto
        {
            TransacaoId = transacao.Id,
            ChaveIdempotencia = transacao.ChaveIdempotencia,
            CorrelationId = Guid.NewGuid().ToString(), // Ideally comes from context, generating here for simplicity
            PayloadOriginal = transacao.PayloadOriginal,
            TentativaAtual = 0,
            DataEnfileiramento = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageDto));

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = transacao.Id.ToString();
        properties.CorrelationId = messageDto.CorrelationId;

        // Publica no channel. IModel.BasicPublish não é assíncrono por padrão na lib antiga do RabbitMQ.
        _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body);

        _logger.LogDebug("Mensagem enfileirada no RabbitMQ: TransacaoId {TransacaoId}", transacao.Id);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
