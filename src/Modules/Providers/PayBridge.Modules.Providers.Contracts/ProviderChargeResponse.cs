using PayBridge.Modules.Providers.Contracts.Enums;

namespace PayBridge.Modules.Providers.Contracts;

public sealed record ProviderChargeResponse(
    ProviderPaymentState State,
    string? ProviderTransactionId,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ProviderChargeResponse Success(
        string providerTransactionId)
    {
        return new ProviderChargeResponse(
            State: ProviderPaymentState.Succeeded,
            ProviderTransactionId: providerTransactionId,
            ErrorCode: null,
            ErrorMessage: null);
    }

    public static ProviderChargeResponse Failed(
        string errorCode,
        string errorMessage)
    {
        return new ProviderChargeResponse(
            State: ProviderPaymentState.Failed,
            ProviderTransactionId: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);
    }

    public static ProviderChargeResponse InquiryRequired(
        string errorCode,
        string errorMessage)
    {
        return new ProviderChargeResponse(
            State: ProviderPaymentState.StillProcessing,
            ProviderTransactionId: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);
    }
}