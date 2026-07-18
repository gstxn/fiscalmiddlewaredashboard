using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace FiscalMiddleware.Infrastructure.HttpClients;

public static class HttpClientExtensions
{
    public static IServiceCollection AddExternalFiscalClientWithPolly(this IServiceCollection services, string baseAddress)
    {
        services.AddHttpClient<FiscalMiddleware.Application.Interfaces.IExternalFiscalClient, ExternalFiscalClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            // Default timeout para a requisição inteira, antes do polly
            client.Timeout = TimeSpan.FromSeconds(30); 
        })
        .AddResilienceHandler("FiscalPipeline", builder =>
        {
            // O Circuit Breaker deve ENVOLVER o Retry. A ordem de registro no builder do Polly (Extensions) 
            // importa. As políticas registradas primeiro são as políticas "mais externas" no pipeline.
            // Para que o CircuitBreaker envolva o Retry, ele deve ser adicionado PRIMEIRO.
            
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5, // Após 50% de falhas
                MinimumThroughput = 5, // com mínimo de 5 tentativas
                SamplingDuration = TimeSpan.FromSeconds(30), // na janela de 30s
                BreakDuration = TimeSpan.FromSeconds(15) // Abre por 15s
            });

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(2) // 2, 4, 8s (base)
            });

            builder.AddTimeout(TimeSpan.FromSeconds(10)); // Timeout por tentativa
        });

        return services;
    }
}
