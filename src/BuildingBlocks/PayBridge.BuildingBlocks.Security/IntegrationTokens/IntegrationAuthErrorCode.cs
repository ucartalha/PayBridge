namespace PayBridge.BuildingBlocks.Security.IntegrationTokens;

public enum IntegrationAuthErrorCode
{
    InvalidClient = 130001,
    InvalidToken = 130002,
    InsufficientScope = 130003,
    MerchantIdRequired = 130004,
    PaymentTargetNotAllowed = 130005
}