namespace FiscalMiddleware.Domain.Enums;

public enum StatusTransacao
{
    Pendente,
    Processando,
    Sucesso,
    Falha,
    EmDLQ
}
