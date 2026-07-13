namespace PayBridge.Api.Exceptions
{
    public class ApiErrorResponse
    {
        public bool IsSuccess { get; init; }
        public int StatusCode { get; init; }
        public string TraceId { get; init; }
        public ApiError Error { get; init; }
    }

    public sealed class ApiError
    {
        public int Code { get; init; }
        public string Key { get; init; } = default!;
        public string Message { get; init; } = default!;
        public IReadOnlyDictionary<string, string[]> ValidationErrors{ get; init; }
    }
}
