namespace PayBridge.Modules.Providers.Contracts;

public sealed record ProviderCredentialContext(
    Guid MerchantProviderAccountId,
    string ProviderCode,
    string CredentialPayloadJson);