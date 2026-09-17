using PayBridge.BuildingBlocks.CQRS;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using PayBridge.BuildingBlocks.Results;

namespace PayBridge.Modules.Payments.Application
    .Payments.PaymentsExecution;

public sealed record PaymentExecutionRequest(
    Guid IntegrationClientId,
    string ClientCode,
    string MerchantCode,
    string OrderId,
    decimal Amount,
    string Currency,
    string ProviderCode,
    string Channel)
    : IIdempotentRequest<Result<PaymentExecutionResult>>
{
    public Result<PaymentExecutionResult>
        CreateInProgressResponse()
    {
        return Result<PaymentExecutionResult>
            .Conflict(
                IdempotencyErrorCodes.InProgress);
    }
}