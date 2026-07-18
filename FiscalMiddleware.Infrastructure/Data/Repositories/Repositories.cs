using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FiscalMiddleware.Domain.Entities;
using FiscalMiddleware.Application.Interfaces;

namespace FiscalMiddleware.Infrastructure.Data.Repositories;

public class TransacaoRepository : ITransacaoRepository
{
    private readonly FiscalDbContext _context;

    public TransacaoRepository(FiscalDbContext context)
    {
        _context = context;
    }

    public async Task SalvarAsync(Transacao transacao, CancellationToken cancellationToken)
    {
        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Transacao transacao, CancellationToken cancellationToken)
    {
        _context.Transacoes.Update(transacao);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Transacao> ObterPorIdAsync(Guid transacaoId, CancellationToken cancellationToken)
    {
        return await _context.Transacoes.FirstOrDefaultAsync(t => t.Id == transacaoId, cancellationToken);
    }
}

public class LoteRepository : ILoteRepository
{
    private readonly FiscalDbContext _context;

    public LoteRepository(FiscalDbContext context)
    {
        _context = context;
    }

    public async Task SalvarAsync(Lote lote, CancellationToken cancellationToken)
    {
        _context.Lotes.Add(lote);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
