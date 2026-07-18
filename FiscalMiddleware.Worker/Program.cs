using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Serilog;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Infrastructure.Data;
using FiscalMiddleware.Infrastructure.Data.Repositories;
using FiscalMiddleware.Infrastructure.Caching;
using FiscalMiddleware.Infrastructure.HttpClients;
using FiscalMiddleware.Worker;

var builder = Host.CreateDefaultBuilder(args);

builder.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(); 
});

builder.ConfigureServices((hostContext, services) =>
{
    var configuration = hostContext.Configuration;

    services.AddDbContext<FiscalDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

    services.AddSingleton<IConnectionMultiplexer>(sp => 
        ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection") ?? "localhost:6379"));

    services.AddScoped<ITransacaoRepository, TransacaoRepository>();
    services.AddSingleton<IIdempotencyService, RedisIdempotencyService>();

    // Registra o client HTTP da API externa com pipeline Polly
    var externalApiUrl = configuration["ExternalApi:BaseUrl"] ?? "http://localhost:8080/";
    services.AddExternalFiscalClientWithPolly(externalApiUrl);

    services.AddHostedService<FiscalMessageConsumer>();
});

var host = builder.Build();
host.Run();
