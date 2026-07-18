using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FiscalMiddleware.Application.DTOs;

namespace FiscalMiddleware.Application.Commands;

public class ReceberLoteCommand : IRequest<ReceberLoteResult>
{
    public LoteDto Lote { get; set; }

    public ReceberLoteCommand(LoteDto lote)
    {
        Lote = lote;
    }
}

public class ReceberLoteResult
{
    public bool Sucesso { get; set; }
    public string LoteId { get; set; }
    public string MensagemErro { get; set; }
    
    public static ReceberLoteResult Success(string loteId) => new ReceberLoteResult { Sucesso = true, LoteId = loteId };
    public static ReceberLoteResult Failure(string erro) => new ReceberLoteResult { Sucesso = false, MensagemErro = erro };
}
