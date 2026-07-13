namespace PayBridge.Modules.Merchants.Contracts.Merchants;
public sealed record MerchantProviderAccountInfo(
        Guid Id,
        Guid MerchantId,
        string ProviderCode,
        bool IsActive,
        bool AllowECommerce,
        bool AllowPhysicalPos,
        bool AllowRefund,
        int Priority);

