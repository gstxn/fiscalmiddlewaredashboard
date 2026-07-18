using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using StackExchange.Redis;
using Serilog;
using FluentValidation;
using FiscalMiddleware.Application.Interfaces;
using FiscalMiddleware.Infrastructure.Data;
using FiscalMiddleware.Infrastructure.Data.Repositories;
using FiscalMiddleware.Infrastructure.Messaging;
using FiscalMiddleware.Infrastructure.Caching;
using FiscalMiddleware.Infrastructure.HttpClients;
using FiscalMiddleware.WebAPI.Middlewares;
using FiscalMiddleware.Application.Commands;
using FiscalMiddleware.Application.Handlers;
using FiscalMiddleware.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(); // Em prod, adicionar seq/elk sink
});

var configuration = builder.Configuration;

// 1. Bancos de Dados
builder.Services.AddDbContext<FiscalDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

// 2. Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection") ?? "localhost:6379"));

// 3. Dependências Core (Application & Infra)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ReceberLoteCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(ReceberLoteCommandValidator).Assembly);

// Repositories
builder.Services.AddScoped<ILoteRepository, LoteRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddSingleton<IIdempotencyService, RedisIdempotencyService>();

var externalApiUrl = configuration["ExternalApi:BaseUrl"] ?? "http://localhost:8080/";
builder.Services.AddExternalFiscalClientWithPolly(externalApiUrl);

builder.Services.AddHostedService<FiscalMessageConsumer>();
// 4. API Controllers & Routing
builder.Services.AddControllers();

// 5. Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("PostgresConnection") ?? "", name: "postgres")
    .AddRedis(configuration.GetConnectionString("RedisConnection") ?? "localhost:6379", name: "redis");

// CORS for React Panel
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalPanel",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowLocalPanel");

app.UseRouting();

// Mapear rota dos controllers
app.MapControllers();

// Rota do HealthCheck
app.MapHealthChecks("/health");

app.Run();
