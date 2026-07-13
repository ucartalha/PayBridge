namespace PayBridge.Modules.Merchants.Domain.Merchants.Errors;

public enum MerchantErrorCode
{
    MerchantIdRequired = 200000,
    MerchantCodeRequired = 200001,
    LegalNameRequired = 200002,
    DisplayNameRequired = 200003,
    TaxNumberRequired = 200004,

    SectorCodeRequired = 200010,
    SectorNameRequired = 200011,
    SectorNotActive = 200012,

    MccCodeRequired = 200020,
    MccDescriptionRequired = 200021,
    MccNotActive = 200022,
    MccRestricted = 200023,

    MerchantNotFound = 200030,
    MerchantNotActive = 200031,
    MerchantSuspended = 200032,
    MerchantClosed = 200033,

    PaymentChannelAlreadyExists = 200040,
    PaymentChannelNotEnabled = 200041,
    InvalidChannelMinAmount = 200042,
    InvalidChannelMaxAmount = 200043,
    InvalidChannelDailyLimit = 200044,
    ChannelAmountBelowMinimum = 200045,
    ChannelAmountLimitExceeded = 200046,

    ProviderCodeRequired = 200050,
    ProviderAccountNotFound = 200051,
    ProviderAccountNotActive = 200052,
    ProviderAccountChannelNotAllowed = 200053,
    ProviderAccountRefundNotAllowed = 200054,
    InvalidProviderAccountPriority = 200055,

    EncryptedCredentialPayloadRequired = 200060,
    EncryptionKeyVersionRequired = 200061,
    ProviderCredentialNotActive = 200062
}