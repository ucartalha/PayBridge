namespace PayBridge.Modules.Merchants.Contracts.Merchants;

//Bu dto asla api dışına açılamaz. Sadece servisler arası iletişimde kullanılabilir.

public sealed record MerchantProviderCredentialInfo(
    Guid Id,
    Guid MerchantProviderAccountId,
    string EncryptedCredentialPayload,
    string EncryptionKeyVersion,
    bool IsActive);