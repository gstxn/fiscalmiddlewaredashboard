using System;
using System.Threading;
using System.Threading.Tasks;

namespace FiscalMiddleware.Application.Interfaces;

public interface IIdempotencyService
{
    Task<bool> AcquireLockAsync(string idempotencyKey, TimeSpan ttl, CancellationToken cancellationToken);
}
