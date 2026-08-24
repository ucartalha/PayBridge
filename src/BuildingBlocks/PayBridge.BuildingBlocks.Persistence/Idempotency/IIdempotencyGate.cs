namespace PayBridge.BuildingBlocks.Persistence.Idempotency;

public interface IIdempotencyGate
{
    Task<IdempotencyGateResult> TryAcquireOrGetAsync(
        string key,
        TimeSpan inFlightTtl,
        CancellationToken cancellationToken);

    Task MarkCompletedAsync(
        string key,
        string leaseToken,
        string responseContent,
        TimeSpan completedTtl,
        CancellationToken cancellationToken);

    Task ReleaseAsync(
        string key,
        string leaseToken,
        CancellationToken cancellationToken);
}