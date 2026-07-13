using Microsoft.AspNetCore.DataProtection;
using PayBridge.Modules.Merchants.Contracts.Credentials;

namespace PayBridge.Modules.Merchants.Infrastructure.Security;

internal sealed class DevelopmentCredentialProtector : ICredentialProtector
{
    private const string PurposePrefix = "PayBridge.MerchantProviderCredentials";

    private readonly IDataProtectionProvider _dataProtectionProvider;

    public DevelopmentCredentialProtector(
        IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }

    public string Protect(
        string credentialPayloadJson,
        string encryptionKeyVersion)
    {
        if (string.IsNullOrWhiteSpace(credentialPayloadJson))
        {
            throw new ArgumentException(
                "Credential payload json cannot be empty.",
                nameof(credentialPayloadJson));
        }

        if (string.IsNullOrWhiteSpace(encryptionKeyVersion))
        {
            throw new ArgumentException(
                "Encryption key version cannot be empty.",
                nameof(encryptionKeyVersion));
        }

        return CreateProtector(encryptionKeyVersion)
            .Protect(credentialPayloadJson);
    }

    public string Unprotect(
        string encryptedCredentialPayload,
        string encryptionKeyVersion)
    {
        if (string.IsNullOrWhiteSpace(encryptedCredentialPayload))
        {
            throw new ArgumentException(
                "Encrypted credential payload cannot be empty.",
                nameof(encryptedCredentialPayload));
        }

        if (string.IsNullOrWhiteSpace(encryptionKeyVersion))
        {
            throw new ArgumentException(
                "Encryption key version cannot be empty.",
                nameof(encryptionKeyVersion));
        }

        return CreateProtector(encryptionKeyVersion)
            .Unprotect(encryptedCredentialPayload);
    }

    private IDataProtector CreateProtector(string encryptionKeyVersion)
    {
        return _dataProtectionProvider.CreateProtector(
            $"{PurposePrefix}.{encryptionKeyVersion}");
    }
}