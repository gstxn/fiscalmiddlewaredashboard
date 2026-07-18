using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using FiscalMiddleware.Application.Interfaces;

namespace FiscalMiddleware.Infrastructure.Caching;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisIdempotencyService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> AcquireLockAsync(string idempotencyKey, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        
        // Usa o SETNX (Set if Not Exists) nativo do Redis
        // Retorna true se a chave foi setada (lock adquirido), false se já existia (duplicidade)
        return await db.StringSetAsync(
            $"idempotency:{idempotencyKey}",
            "locked",
            ttl,
            When.NotExists
        );
    }
}
