namespace PayBridge.BuildingBlocks.Persistence.Idempotency;

public enum IdempotencyGateStatus
{
    Acquired = 1,
    InFlight = 2,
    Completed = 3,
    Unavailable = 4
}