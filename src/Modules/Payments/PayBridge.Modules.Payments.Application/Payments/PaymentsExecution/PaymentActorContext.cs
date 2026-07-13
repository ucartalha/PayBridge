namespace PayBridge.Modules.Payments.Application
    .Payments.PaymentsExecution;

public sealed record PaymentActorContext(
    Guid IntegrationClientId,
    string ClientCode,
    Guid MerchantId,
    string MerchantCode,
    string MerchantDisplayName,
    string Channel);