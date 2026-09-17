using MediatR;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using System.Text.Json;

namespace PayBridge.BuildingBlocks.CQRS.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private static readonly TimeSpan InFlightTtl =
        TimeSpan.FromSeconds(60);

    private static readonly TimeSpan CompletedTtl =
        TimeSpan.FromMinutes(30);

    private readonly IIdempotencyGate
        _idempotencyGate;

    private readonly IIdempotencyService
        _idempotencyService;

    public IdempotencyBehavior(
        IIdempotencyGate idempotencyGate,
        IIdempotencyService idempotencyService)
    {
        _idempotencyGate =
            idempotencyGate;

        _idempotencyService =
            idempotencyService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not
            IIdempotentRequest<TResponse>
            idempotentRequest)
        {
            return await next();
        }

        var idempotencyKey =
            GenerateIdempotencyKey(request);

        var gateResult =
            await _idempotencyGate
                .TryAcquireOrGetAsync(
                    idempotencyKey,
                    InFlightTtl,
                    cancellationToken);

        switch (gateResult.Status)
        {
            case IdempotencyGateStatus.Completed:
                return DeserializeResponse(
                    gateResult.ResponseContent);

            case IdempotencyGateStatus.InFlight:

                // NORMAL RESPONSE.
                // Exception yok.
                return idempotentRequest
                    .CreateInProgressResponse();

            case IdempotencyGateStatus.Unavailable:
                return await ExecuteWithSqlFallbackAsync(
                    idempotentRequest,
                    idempotencyKey,
                    next,
                    cancellationToken);

            case IdempotencyGateStatus.Acquired:
                return await ExecuteAsOwnerAsync(
                    idempotentRequest,
                    idempotencyKey,
                    gateResult.LeaseToken,
                    next,
                    cancellationToken);

            default:
                throw new InvalidOperationException(
                    $"Unknown idempotency gate status: {gateResult.Status}");
        }
    }

    private async Task<TResponse> ExecuteAsOwnerAsync(
        IIdempotentRequest<TResponse> request,
        string key,
        string? leaseToken,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(leaseToken))
        {
            throw new InvalidOperationException(
                "Acquired Redis gate does not contain a lease token.");
        }

        try
        {
            var storeResult =
                await _idempotencyService
                    .TryAcquireOrGetAsync(
                        key,
                        cancellationToken);

            switch (storeResult.Status)
            {
                case IdempotencyStoreStatus.Completed:
                    {
                        await _idempotencyGate
                            .MarkCompletedAsync(
                                key,
                                leaseToken,
                                storeResult.ResponseContent!,
                                CompletedTtl,
                                cancellationToken);

                        return DeserializeResponse(
                            storeResult.ResponseContent);
                    }

                case IdempotencyStoreStatus.InFlight:
                    {
                        await _idempotencyGate.ReleaseAsync(
                            key,
                            leaseToken,
                            CancellationToken.None);

                        return request
                            .CreateInProgressResponse();
                    }

                case IdempotencyStoreStatus.Acquired:
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown SQL idempotency status: {storeResult.Status}");
            }

            // Bundan sonrası gerçek business workflow.
            var response = await next();

            // Success VE deterministic BusinessFailure
            // burada normal TResponse olarak gelir.

            // Önce SQL source of truth.
            await _idempotencyService.CompleteAsync(
                key,
                response,
                cancellationToken);

            var serializedResponse =
                JsonSerializer.Serialize(response);

            // Sonra Redis fast cache.
            await _idempotencyGate.MarkCompletedAsync(
                key,
                leaseToken,
                serializedResponse,
                CompletedTtl,
                cancellationToken);

            return response;
        }
        catch
        {
            // Unexpected technical failure.
            //
            // SQL InFlight'i burada silmiyoruz.
            // Provider'a istek gitmiş olabileceğinden
            // duplicate charge riski yaratmak istemiyoruz.

            await _idempotencyGate.ReleaseAsync(
                key,
                leaseToken,
                CancellationToken.None);

            throw;
        }
    }

    private async Task<TResponse>
        ExecuteWithSqlFallbackAsync(
            IIdempotentRequest<TResponse> request,
            string key,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
    {
        var storeResult =
            await _idempotencyService
                .TryAcquireOrGetAsync(
                    key,
                    cancellationToken);

        switch (storeResult.Status)
        {
            case IdempotencyStoreStatus.Completed:
                return DeserializeResponse(
                    storeResult.ResponseContent);

            case IdempotencyStoreStatus.InFlight:
                return request
                    .CreateInProgressResponse();

            case IdempotencyStoreStatus.Acquired:
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown SQL idempotency status: {storeResult.Status}");
        }

        var response = await next();

        await _idempotencyService.CompleteAsync(
            key,
            response,
            cancellationToken);

        return response;
    }

    private static string GenerateIdempotencyKey(
        TRequest request)
    {
        var properties = request
            .GetType()
            .GetProperties()
            .Select(x => x.GetValue(request))
            .Where(x => x is not null)
            .ToArray();

        var prefix = request
            .GetType()
            .Name
            .Replace("Command", "")
            .ToLowerInvariant();

        return IdempotencyKeyGenerator
            .GenerateIdempotencyKey(
                prefix,
                properties);
    }

    private static TResponse DeserializeResponse(
        string? responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new InvalidOperationException(
                "Stored idempotency response is empty.");
        }

        return JsonSerializer
            .Deserialize<TResponse>(responseContent)
            ?? throw new InvalidOperationException(
                "Stored idempotency response could not be deserialized.");
    }
}