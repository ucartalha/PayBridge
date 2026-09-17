namespace PayBridge.BuildingBlocks.Results;

public sealed record Result<T>(
    ResultStatus Status,
    T? Value,
    int? ErrorCode)
{
    public bool IsSuccess =>
        Status == ResultStatus.Success;

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(
            ResultStatus.Success,
            value,
            null);
    }

    public static Result<T> BusinessFailure(
        int errorCode)
    {
        return new Result<T>(
            ResultStatus.BusinessFailure,
            default,
            errorCode);
    }

    public static Result<T> Conflict(
        int errorCode)
    {
        return new Result<T>(
            ResultStatus.Conflict,
            default,
            errorCode);
    }
}