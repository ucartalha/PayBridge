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
    string Channel);