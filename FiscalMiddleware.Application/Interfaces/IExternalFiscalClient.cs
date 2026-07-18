using System.Threading;
using System.Threading.Tasks;

namespace FiscalMiddleware.Application.Interfaces;

public interface IExternalFiscalClient
{
    /// <summary>
    /// Envia a transação para o sistema externo.
    /// Retorna o status HTTP da resposta (ex: 200, 400, 500) e opcionalmente uma string detalhando erro.
    /// </summary>
    Task<(int StatusCode, string ErrorDetail)> EnviarTransacaoAsync(string payload, CancellationToken cancellationToken);
}
