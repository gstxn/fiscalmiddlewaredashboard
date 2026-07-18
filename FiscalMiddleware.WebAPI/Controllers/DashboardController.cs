using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FiscalMiddleware.Application.Queries;

namespace FiscalMiddleware.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), 200)]
    public async Task<IActionResult> ObterEstatisticas(CancellationToken cancellationToken)
    {
        var query = new ObterEstatisticasDashboardQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
