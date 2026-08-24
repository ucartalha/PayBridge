using PayBridge.BuildingBlocks.CQRS;

namespace PayBridge.Modules.Payments.Application.Payments.PaymentsExecution.Command
{
    public sealed record PaymentExecutionCommand(
    Guid IntegrationClientId,
    string ClientCode,
    string MerchantCode,
    string OrderId,
    decimal Amount,
    string Currency,
    string ProviderCode,
    string Channel)
    : ICommand<PaymentExecutionResult>,
      IIdempotentCommand<PaymentExecutionResult>;
}
