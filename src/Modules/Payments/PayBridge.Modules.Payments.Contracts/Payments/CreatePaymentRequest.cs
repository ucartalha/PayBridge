namespace PayBridge.Api.Contracts.Payments;

public sealed record CreatePaymentRequest(
    string MerchantCode,
    string OrderId,
    decimal Amount,
    string Currency,
    string ProviderCode,
    string Channel);