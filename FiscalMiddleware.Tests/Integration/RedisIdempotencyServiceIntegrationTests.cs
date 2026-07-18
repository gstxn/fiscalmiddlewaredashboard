using System;
using System.Threading;
using System.Threading.Tasks;
using FiscalMiddleware.Infrastructure.Caching;
using FluentAssertions;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace FiscalMiddleware.Tests.Integration;

public class RedisIdempotencyServiceIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;
    private IConnectionMultiplexer _connection;
    private RedisIdempotencyService _service;

    public RedisIdempotencyServiceIntegrationTests()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
        _connection = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        _service = new RedisIdempotencyService(_connection);
    }

    public async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task Deve_Adquirir_Lock_Na_Primeira_Vez_E_Falhar_Na_Segunda()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var ttl = TimeSpan.FromSeconds(30);

        // Act
        var firstAttempt = await _service.AcquireLockAsync(key, ttl, CancellationToken.None);
        var secondAttempt = await _service.AcquireLockAsync(key, ttl, CancellationToken.None);

        // Assert
        firstAttempt.Should().BeTrue("A primeira tentativa deve conseguir o lock.");
        secondAttempt.Should().BeFalse("A segunda tentativa deve falhar pois já existe no cache.");
    }
}
