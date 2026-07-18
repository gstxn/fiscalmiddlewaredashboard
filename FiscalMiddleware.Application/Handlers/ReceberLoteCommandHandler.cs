using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using FiscalMiddleware.Application.Commands;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Domain.Entities;
using FiscalMiddleware.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FiscalMiddleware.Application.Handlers;

public class ReceberLoteCommandHandler : IRequestHandler<ReceberLoteCommand, ReceberLoteResult>
{
    private readonly ILoteRepository _loteRepository;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IValidator<ReceberLoteCommand> _validator;
    private readonly ILogger<ReceberLoteCommandHandler> _logger;

    public ReceberLoteCommandHandler(
        ILoteRepository loteRepository,
        ITransacaoRepository transacaoRepository,
        IMessagePublisher messagePublisher,
        IValidator<ReceberLoteCommand> validator,
        ILogger<ReceberLoteCommandHandler> logger)
    {
        _loteRepository = loteRepository;
        _transacaoRepository = transacaoRepository;
        _messagePublisher = messagePublisher;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ReceberLoteResult> Handle(ReceberLoteCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Falha na validação do lote da origem {Origem}", request.Lote?.Origem);
            // Ideally, we'd return structured errors, but for simplicity returning a combined string
            return ReceberLoteResult.Failure(string.Join("; ", validationResult.Errors));
        }

        Guid.TryParse(request.Lote.LoteId, out var loteIdParsed);
        var loteId = loteIdParsed == Guid.Empty ? Guid.NewGuid() : loteIdParsed;

        var lote = new Lote(loteId, request.Lote.Origem);

        foreach (var tDto in request.Lote.Transacoes)
        {
            // O ideal seria serializar o payload original se for um objeto dinâmico
            var payloadStr = System.Text.Json.JsonSerializer.Serialize(tDto.PayloadOriginal);
            
            var transacao = new Transacao(
                loteId,
                tDto.DocumentoFiscal,
                tDto.CnpjEmitente,
                tDto.Valor,
                tDto.TipoOperacao,
                payloadStr
            );
            
            lote.AdicionarTransacao(transacao);
        }

        // Salva o lote e as transações no banco
        await _loteRepository.SalvarAsync(lote, cancellationToken);

        // Publica cada transação na fila do RabbitMQ
        foreach (var transacao in lote.Transacoes)
        {
            await _messagePublisher.PublicarTransacaoAsync(transacao, cancellationToken);
            _logger.LogInformation("Transação {TransacaoId} do Lote {LoteId} enfileirada com sucesso.", transacao.Id, lote.Id);
        }

        _logger.LogInformation("Lote {LoteId} processado e enfileirado.", lote.Id);

        return ReceberLoteResult.Success(lote.Id.ToString());
    }
}
