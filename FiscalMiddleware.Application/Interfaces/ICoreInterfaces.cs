using System;
using System.Threading;
using System.Threading.Tasks;
using FiscalMiddleware.Domain.Entities;

namespace FiscalMiddleware.Application.Interfaces;

public interface ITransacaoRepository
{
    Task SalvarAsync(Transacao transacao, CancellationToken cancellationToken);
    Task AtualizarAsync(Transacao transacao, CancellationToken cancellationToken);
    Task<Transacao> ObterPorIdAsync(Guid transacaoId, CancellationToken cancellationToken);
}

public interface ILoteRepository
{
    Task SalvarAsync(Lote lote, CancellationToken cancellationToken);
}

public interface IMessagePublisher
{
    Task PublicarTransacaoAsync(Transacao transacao, CancellationToken cancellationToken);
}
