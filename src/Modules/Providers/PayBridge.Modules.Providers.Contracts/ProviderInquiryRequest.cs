namespace PayBridge.Modules.Providers.Contracts;

public sealed record ProviderInquiryRequest(
    Guid PaymentId,
    string OrderId,
    decimal Amount,
    string Currency,
    string? ProviderTransactionId,
    int AttemptNumber,
    ProviderCredentialContext? Credential =null);