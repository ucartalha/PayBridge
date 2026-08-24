namespace PayBridge.BuildingBlocks.Persistence.Idempotency;

public interface IIdempotencyService
{
    Task<string?> TryAcquireOrGetCompletedResultAsync(
        string key,
        CancellationToken cancellationToken);

    Task CompleteAsync(
        string key,
        object result,
        CancellationToken cancellationToken);
}