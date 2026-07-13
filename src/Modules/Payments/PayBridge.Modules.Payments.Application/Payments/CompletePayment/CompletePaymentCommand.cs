using PayBridge.BuildingBlocks.CQRS;
using PayBridge.Modules.Providers.Contracts.Enums;

namespace PayBridge.Modules.Payments.Application.Payments.CompletePayment
{
    public sealed record CompletePaymentCommand(
     Guid PaymentId,
     ProviderPaymentState ProviderState,
     string? ProviderTransactionId,
     string? ErrorCode,
     string? ErrorMessage)
     : ICommand<CompletePaymentResult>;
}
