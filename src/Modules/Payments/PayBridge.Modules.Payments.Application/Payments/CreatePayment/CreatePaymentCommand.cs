using PayBridge.BuildingBlocks.CQRS;

namespace PayBridge.Modules.Payments.Application.Payments.CreatePayment;

public sealed record CreatePaymentCommand(
    Guid MerchantId,
    string OrderId,
    decimal Amount,
    string Currency,
    string ProviderCode) : ICommand<CreatePaymentResult>, IIdempotentCommand<CreatePaymentResult>;