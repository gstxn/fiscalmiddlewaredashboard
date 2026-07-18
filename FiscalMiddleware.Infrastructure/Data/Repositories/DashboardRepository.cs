using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Application.Queries;

namespace FiscalMiddleware.Infrastructure.Data.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly FiscalDbContext _context;

    public DashboardRepository(FiscalDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> ObterEstatisticasAsync(CancellationToken cancellationToken)
    {
        var totalPendentes = await _context.Transacoes.CountAsync(t => t.Status == Domain.Enums.StatusTransacao.Pendente, cancellationToken);
        var totalProcessando = await _context.Transacoes.CountAsync(t => t.Status == Domain.Enums.StatusTransacao.Processando, cancellationToken);
        var totalDlq = await _context.Transacoes.CountAsync(t => t.Status == Domain.Enums.StatusTransacao.EmDLQ, cancellationToken);

        var limitDate = DateTime.UtcNow.AddDays(-1);
        var sucessoCount = await _context.Historicos.CountAsync(h => h.Timestamp >= limitDate && h.StatusAlcancado == Domain.Enums.StatusTransacao.Sucesso, cancellationToken);
        var falhaCount = await _context.Historicos.CountAsync(h => h.Timestamp >= limitDate && h.StatusAlcancado == Domain.Enums.StatusTransacao.Falha, cancellationToken);

        var total24h = sucessoCount + falhaCount;
        var taxaSucesso = total24h > 0 ? (sucessoCount * 100.0 / total24h).ToString("F1") + "%" : "0.0%";
        var taxaFalha = total24h > 0 ? (falhaCount * 100.0 / total24h).ToString("F1") + "%" : "0.0%";

        return new DashboardStatsDto
        {
            MensagensPendentes = totalPendentes,
            MensagensProcessando = totalProcessando,
            TaxaSucesso24h = taxaSucesso,
            TaxaFalha24h = taxaFalha,
            DlqCount = totalDlq
        };
    }
}
