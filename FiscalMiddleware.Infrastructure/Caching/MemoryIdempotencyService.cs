using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using FiscalMiddleware.Application.Interfaces;

namespace FiscalMiddleware.Infrastructure.Caching;

public class MemoryIdempotencyService : IIdempotencyService
{
    private readonly IMemoryCache _cache;

    public MemoryIdempotencyService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<bool> AcquireLockAsync(string idempotencyKey, TimeSpan ttl, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(idempotencyKey, out _))
        {
            return Task.FromResult(false); // Já existe
        }

        _cache.Set(idempotencyKey, true, ttl);
        return Task.FromResult(true); // Bloqueio adquirido
    }
}
