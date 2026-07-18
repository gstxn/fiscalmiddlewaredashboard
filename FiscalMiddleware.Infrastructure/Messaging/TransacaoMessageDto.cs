using System;

namespace FiscalMiddleware.Infrastructure.Messaging;

public class TransacaoMessageDto
{
    public Guid TransacaoId { get; set; }
    public string ChaveIdempotencia { get; set; }
    public string CorrelationId { get; set; }
    public string PayloadOriginal { get; set; }
    public int TentativaAtual { get; set; }
    public DateTime DataEnfileiramento { get; set; }
}
