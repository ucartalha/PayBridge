namespace PayBridge.Modules.Providers.Contracts
{
    public sealed record ProviderChargeRequest(
     Guid PaymentId,
     string OrderId,
     decimal Amount,
     string Currency,
     string IdempotencyKey,
     ProviderCredentialContext? Credential = null);
}
