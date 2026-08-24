namespace PayBridge.BuildingBlocks.Persistence.Idempotency;

public sealed record IdempotencyGateResult(
    IdempotencyGateStatus Status,
    string? LeaseToken = null,
    string? ResponseContent = null)
{
    public static IdempotencyGateResult Acquired(
        string leaseToken)
        => new(
            IdempotencyGateStatus.Acquired,
            LeaseToken: leaseToken);

    public static IdempotencyGateResult InFlight()
        => new(
            IdempotencyGateStatus.InFlight);

    public static IdempotencyGateResult Completed(
        string responseContent)
        => new(
            IdempotencyGateStatus.Completed,
            ResponseContent: responseContent);

    public static IdempotencyGateResult Unavailable()
        => new(
            IdempotencyGateStatus.Unavailable);
}