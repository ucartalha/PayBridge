using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Persistence.Idempotency
{
    public interface IIdempotencyService
    {
        Task<string?> GetInFlightOrCompletedResultAsync(string key, CancellationToken cancellationToken);
        Task CreateInFlightAsync(string key, CancellationToken cancellationToken);
        Task CompleteAsync(string key, object result, CancellationToken cancellationToken);
    }
}
