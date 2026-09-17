namespace PayBridge.BuildingBlocks.Persistence.Idempotency;

public sealed record IdempotencyStoreResult(
    IdempotencyStoreStatus Status,
    string? ResponseContent = null)
{
    public static IdempotencyStoreResult Acquired()
        => new(IdempotencyStoreStatus.Acquired);

    public static IdempotencyStoreResult InFlight()
        => new(IdempotencyStoreStatus.InFlight);

    public static IdempotencyStoreResult Completed(
        string responseContent)
        => new(
            IdempotencyStoreStatus.Completed,
            responseContent);
}