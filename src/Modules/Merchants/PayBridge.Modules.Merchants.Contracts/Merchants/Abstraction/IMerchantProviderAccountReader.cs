namespace PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction;

public interface IMerchantProviderAccountReader
{
    Task<MerchantProviderAccountInfo?> GetActiveProviderAccountAsync(
        Guid merchantId,
        string providerCode,
        string channel,
        CancellationToken cancellationToken = default);

    Task<MerchantProviderCredentialInfo?> GetActiveCredentialAsync(
        Guid merchantProviderAccountId,
        CancellationToken cancellationToken = default);
}