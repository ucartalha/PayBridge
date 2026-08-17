using Microsoft.Extensions.Primitives;

namespace PayBridge.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private const int MaxCorrelationIdLength = 100;

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.TraceIdentifier = correlationId;
        context.Items[HeaderName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using var scope = logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

        await _next(context);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(
                HeaderName,
                out StringValues headerValues))
        {
            return CreateCorrelationId();
        }

        var correlationId = headerValues.FirstOrDefault()?.Trim();

        if (string.IsNullOrWhiteSpace(correlationId) ||
            correlationId.Length > MaxCorrelationIdLength)
        {
            return CreateCorrelationId();
        }

        return correlationId;
    }

    private static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }
}