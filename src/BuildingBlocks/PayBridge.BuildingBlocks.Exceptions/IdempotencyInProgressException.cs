namespace PayBridge.BuildingBlocks.Exceptions;

public sealed class IdempotencyInProgressException : Exception
{
    public IdempotencyInProgressException()
        : base("An operation with the same idempotency key is already in progress.")
    {
    }
}