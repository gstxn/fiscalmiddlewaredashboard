using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FiscalMiddleware.Application.Interfaces;

namespace FiscalMiddleware.Infrastructure.HttpClients;

public class ExternalFiscalClient : IExternalFiscalClient
{
    private readonly HttpClient _httpClient;

    public ExternalFiscalClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(int StatusCode, string ErrorDetail)> EnviarTransacaoAsync(string payload, CancellationToken cancellationToken)
    {
        try
        {
            // --- SIMULAÇÃO PARA DEMONSTRAÇÃO DO PORTFÓLIO ---
            // Como api.fake-fiscal-gateway.com não existe, vamos simular uma resposta
            await Task.Delay(1500, cancellationToken); // Simula latência de rede
            
            var rand = new Random().Next(1, 100);
            
            if (rand <= 70) 
            {
                // 70% de chance de Sucesso
                return (200, null);
            }
            else if (rand <= 90)
            {
                // 20% de chance de erro interno (vai pra retry e depois DLQ)
                return (500, "Erro interno simulado no órgão fiscal");
            }
            else
            {
                // 10% de chance de falha de rede
                throw new HttpRequestException("Simulação de falha de rede");
            }
        }
        catch (HttpRequestException ex)
        {
            return (503, $"Falha de rede ao acessar API Externa: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return (408, $"Timeout ao acessar API Externa: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (500, $"Erro interno ao tentar chamar a API externa: {ex.Message}");
        }
    }
}
