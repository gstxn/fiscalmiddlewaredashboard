using System.Collections.Generic;
using FiscalMiddleware.Domain.Enums;

namespace FiscalMiddleware.Application.DTOs;

public class LoteDto
{
    public string LoteId { get; set; }
    public string Origem { get; set; }
    public List<TransacaoDto> Transacoes { get; set; } = new();
}

public class TransacaoDto
{
    public string DocumentoFiscal { get; set; }
    public string CnpjEmitente { get; set; }
    public decimal Valor { get; set; }
    public TipoOperacao TipoOperacao { get; set; }
    public object PayloadOriginal { get; set; }
}
