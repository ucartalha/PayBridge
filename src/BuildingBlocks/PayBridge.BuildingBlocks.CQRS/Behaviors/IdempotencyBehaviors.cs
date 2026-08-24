using MediatR;
using PayBridge.BuildingBlocks.Exceptions;
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

    private readonly IIdempotencyGate _idempotencyGate;
    private readonly IIdempotencyService _idempotencyService;

    public IdempotencyBehavior(
        IIdempotencyGate idempotencyGate,
        IIdempotencyService idempotencyService)
    {
        _idempotencyGate = idempotencyGate;
        _idempotencyService = idempotencyService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentRequest<TResponse>)
        {
            return await next();
        }

        var idempotencyKey =
            GenerateIdempotencyKey(request);

        var gateResult =
            await _idempotencyGate.TryAcquireOrGetAsync(
                idempotencyKey,
                InFlightTtl,
                cancellationToken);

        switch (gateResult.Status)
        {
            case IdempotencyGateStatus.Completed:
                return DeserializeResponse(
                    gateResult.ResponseContent);

            case IdempotencyGateStatus.InFlight:
                throw new IdempotencyInProgressException();

            case IdempotencyGateStatus.Unavailable:
                return await ExecuteWithSqlFallbackAsync(
                    idempotencyKey,
                    next,
                    cancellationToken);

            case IdempotencyGateStatus.Acquired:
                return await ExecuteAsGateOwnerAsync(
                    idempotencyKey,
                    gateResult.LeaseToken,
                    next,
                    cancellationToken);

            default:
                throw new InvalidOperationException(
                    $"Unknown idempotency gate status: {gateResult.Status}");
        }
    }

    private async Task<TResponse> ExecuteAsGateOwnerAsync(
        string idempotencyKey,
        string? leaseToken,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(leaseToken))
        {
            throw new InvalidOperationException(
                "An acquired idempotency gate must contain a lease token.");
        }

        try
        {
            // Redis gate'i kazandık fakat SQL hala source of truth.
            //
            // Örneğin:
            // Redis restart etmiş olabilir,
            // fakat aynı key SQL'de daha önce Completed olabilir.
            var completedResult =
                await _idempotencyService
                    .TryAcquireOrGetCompletedResultAsync(
                        idempotencyKey,
                        cancellationToken);

            if (completedResult is not null)
            {
                // SQL'de Completed bulduk.
                // Redis cache'i yeniden dolduruyoruz.
                await _idempotencyGate.MarkCompletedAsync(
                    idempotencyKey,
                    leaseToken,
                    completedResult,
                    CompletedTtl,
                    cancellationToken);

                return DeserializeResponse(
                    completedResult);
            }

            // Hem Redis gate hem SQL ownership bu request'te.
            // Artık gerçek workflow çalışabilir.
            var response = await next();

            // ÖNCE durable store.
            await _idempotencyService.CompleteAsync(
                idempotencyKey,
                response,
                cancellationToken);

            var serializedResponse =
                JsonSerializer.Serialize(response);

            // SONRA Redis cache.
            await _idempotencyGate.MarkCompletedAsync(
                idempotencyKey,
                leaseToken,
                serializedResponse,
                CompletedTtl,
                cancellationToken);

            return response;
        }
        catch
        {
            // Workflow başarısız olduysa Redis gate'i gereksiz
            // şekilde TTL süresi boyunca tutmayalım.
            //
            // CancellationToken.None kullanmamız bilinçli:
            // HTTP request cancel edilmiş olsa bile best-effort
            // olarak kendi lease'imizi bırakmak istiyoruz.
            await _idempotencyGate.ReleaseAsync(
                idempotencyKey,
                leaseToken,
                CancellationToken.None);

            throw;
        }
    }

    private async Task<TResponse> ExecuteWithSqlFallbackAsync(
        string idempotencyKey,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Redis unavailable olduğunda mevcut SQL tabanlı
        // correctness mekanizması devrede kalır.
        var completedResult =
            await _idempotencyService
                .TryAcquireOrGetCompletedResultAsync(
                    idempotencyKey,
                    cancellationToken);

        if (completedResult is not null)
        {
            return DeserializeResponse(
                completedResult);
        }

        var response = await next();

        await _idempotencyService.CompleteAsync(
            idempotencyKey,
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
            .Select(property =>
                property.GetValue(request))
            .Where(value =>
                value is not null)
            .ToArray();

        // ÖNEMLİ:
        // Burada mevcut key formatımızı değiştirmiyoruz.
        // "Request" kelimesini kaldırmıyoruz.
        //
        // PaymentExecutionRequest:
        // paymentexecutionrequest:<hash>
        //
        // Böylece DB'deki mevcut idempotency kayıtlarıyla
        // geriye dönük uyumluluk korunur.
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
            .Deserialize<TResponse>(
                responseContent)
            ?? throw new InvalidOperationException(
                "Stored idempotency response could not be deserialized.");
    }
}