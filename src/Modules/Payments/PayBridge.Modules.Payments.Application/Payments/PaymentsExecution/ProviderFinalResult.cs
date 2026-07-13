using PayBridge.Modules.Providers.Contracts;
using PayBridge.Modules.Providers.Contracts.Enums;

namespace PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;

public sealed record ProviderFinalResult(
    ProviderPaymentState State,
    string? ProviderTransactionId,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ProviderFinalResult Success(
        string providerTransactionId)
    {
        return new ProviderFinalResult(
            State: ProviderPaymentState.Succeeded,
            ProviderTransactionId: providerTransactionId,
            ErrorCode: null,
            ErrorMessage: null);
    }

    public static ProviderFinalResult Failed(
        string errorCode,
        string errorMessage)
    {
        return new ProviderFinalResult(
            State: ProviderPaymentState.Failed,
            ProviderTransactionId: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);
    }

    public static ProviderFinalResult StillProcessing(
        string errorCode,
        string errorMessage)
    {
        return new ProviderFinalResult(
            State: ProviderPaymentState.StillProcessing,
            ProviderTransactionId: null,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);
    }
}