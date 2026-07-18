using System.Threading;
using System.Threading.Tasks;
using FiscalMiddleware.Application.Queries;

namespace FiscalMiddleware.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardStatsDto> ObterEstatisticasAsync(CancellationToken cancellationToken);
}
