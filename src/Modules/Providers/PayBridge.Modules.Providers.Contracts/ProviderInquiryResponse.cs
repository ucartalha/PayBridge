using PayBridge.Modules.Providers.Contracts.Enums;

namespace PayBridge.Modules.Providers.Contracts;

public sealed record ProviderInquiryResponse(
    ProviderPaymentState State,
    string? ProviderTransactionId,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ProviderInquiryResponse Success(
        string providerTransactionId)
    {
        return new ProviderInquiryResponse(
            State: ProviderPaymentState.Succeeded,
            ProviderTransactionId: providerTransactionId,
            ErrorCode: null,
            ErrorMessage: null);
    }

    public static ProviderInquiryResponse Failed(
        string errorCode,
        string errorMessage)
    {
        return new ProviderInquiryResponse(
            State: ProviderPaymentState.Failed,
            ProviderTransactionId: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);
    }

    public static ProviderInquiryResponse StillProcessing(
        string errorCode,
        string errorMessage)
    {
        return new ProviderInquiryResponse(
            State: ProviderPaymentState.StillProcessing,
            ProviderTransactionId: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);
    }
}