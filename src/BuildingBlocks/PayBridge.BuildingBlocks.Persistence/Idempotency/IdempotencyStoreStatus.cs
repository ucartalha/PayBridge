namespace PayBridge.BuildingBlocks.Persistence.Idempotency;

public enum IdempotencyStoreStatus
{
    Acquired = 1,
    InFlight = 2,
    Completed = 3
}