using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FiscalMiddleware.Application.Commands;
using FiscalMiddleware.Application.DTOs;

namespace FiscalMiddleware.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TransacoesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransacoesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("lote")]
    public async Task<IActionResult> ReceberLote([FromBody] LoteDto loteDto, CancellationToken cancellationToken)
    {
        var command = new ReceberLoteCommand(loteDto);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Sucesso)
        {
            return Accepted(new { LoteId = result.LoteId, Mensagem = "Lote recebido e em processamento." });
        }

        return BadRequest(new { Erro = result.MensagemErro });
    }
}
