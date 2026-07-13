namespace PayBridge.Modules.Merchants.Contracts.Merchants;

public sealed record MerchantChannelSettingInfo(
    Guid Id,
    Guid MerchantId,
    string Channel,
    bool IsEnabled,
    decimal MinAmount,
    decimal MaxAmount,
    decimal? DailyAmountLimit,
    bool Require3DS,
    bool AllowRefund,
    bool AllowVoid);