using Elastic.Apm;
using PayBridge.BuildingBlocks.CQRS.Observability;


namespace PayBridge.Api.Observability;

public sealed class ElasticApplicationTracer : IApplicationTracer
{
    public async Task<T> CaptureSpanAsync<T>(
        string name,
        string type,
        Func<Task<T>> operation)
    {
        var transaction = Agent.Tracer.CurrentTransaction;

        if (transaction is null)
        {
            return await operation();
        }

        return await transaction.CaptureSpan(
            name,
            type,
            async span =>
            {
                try
                {
                    return await operation();
                }
                catch (Exception exception)
                {
                    span.CaptureException(exception);
                    throw;
                }
            });
    }
}