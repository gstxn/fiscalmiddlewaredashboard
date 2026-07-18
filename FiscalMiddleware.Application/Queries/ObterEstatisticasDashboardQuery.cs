using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FiscalMiddleware.Application.Interfaces;

namespace FiscalMiddleware.Application.Queries;

public class ObterEstatisticasDashboardQuery : IRequest<DashboardStatsDto> { }

public class DashboardStatsDto
{
    public int MensagensPendentes { get; set; }
    public int MensagensProcessando { get; set; }
    public string TaxaSucesso24h { get; set; }
    public string TaxaFalha24h { get; set; }
    public int DlqCount { get; set; }
}

public class ObterEstatisticasDashboardQueryHandler : IRequestHandler<ObterEstatisticasDashboardQuery, DashboardStatsDto>
{
    private readonly IDashboardRepository _repository;

    public ObterEstatisticasDashboardQueryHandler(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardStatsDto> Handle(ObterEstatisticasDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _repository.ObterEstatisticasAsync(cancellationToken);
    }
}
