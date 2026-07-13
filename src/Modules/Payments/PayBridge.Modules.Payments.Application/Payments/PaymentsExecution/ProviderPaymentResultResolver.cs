using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Providers.Contracts;
using PayBridge.Modules.Providers.Contracts.Enums;

namespace PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;

public sealed class ProviderPaymentResultResolver : IProviderPaymentResultResolver
{
    private static readonly TimeSpan[] InquiryDelays =
    [
        TimeSpan.Zero,
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(1000)
    ];

    public async Task<ProviderFinalResult> ResolveAsync(
        IPaymentProvider provider,
        ProviderChargeRequest chargeRequest,
        CancellationToken cancellationToken = default)
    {
        var chargeResponse = await ChargeSafelyAsync(
            provider,
            chargeRequest,
            cancellationToken);

        return chargeResponse.State switch
        {
            ProviderPaymentState.Succeeded =>
                ToSuccessOrStillProcessing(
                    chargeResponse.ProviderTransactionId,
                    "PROVIDER_TRANSACTION_ID_MISSING",
                    "Provider returned success but provider transaction id is missing."),

            ProviderPaymentState.Failed =>
                ProviderFinalResult.Failed(
                    chargeResponse.ErrorCode ?? "PROVIDER_PAYMENT_FAILED",
                    chargeResponse.ErrorMessage ?? "Provider payment failed."),

            ProviderPaymentState.StillProcessing =>
                await TryResolveWithShortInquiryAsync(
                    provider,
                    chargeRequest,
                    chargeResponse.ProviderTransactionId,
                    cancellationToken),

            _ =>
                ProviderFinalResult.StillProcessing(
                    "PROVIDER_UNKNOWN_STATE",
                    $"Provider returned unknown state: {chargeResponse.State}")
        };
    }

    private static async Task<ProviderChargeResponse> ChargeSafelyAsync(
        IPaymentProvider provider,
        ProviderChargeRequest chargeRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await provider.ChargeAsync(
                chargeRequest,
                cancellationToken);
        }
        catch (TimeoutException ex)
        {
            return ProviderChargeResponse.InquiryRequired(
                errorCode: "PROVIDER_TIMEOUT",
                errorMessage: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return ProviderChargeResponse.InquiryRequired(
                errorCode: "PROVIDER_TIMEOUT",
                errorMessage: ex.Message);
        }
    }

    private static async Task<ProviderFinalResult> TryResolveWithShortInquiryAsync(
        IPaymentProvider provider,
        ProviderChargeRequest chargeRequest,
        string? providerTransactionId,
        CancellationToken cancellationToken)
    {
        ProviderInquiryResponse? lastInquiryResponse = null;

        for (var index = 0; index < InquiryDelays.Length; index++)
        {
            var delay = InquiryDelays[index];

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            var attemptNumber = index + 1;

            var inquiryResponse = await InquirySafelyAsync(
                provider,
                new ProviderInquiryRequest(
                    PaymentId: chargeRequest.PaymentId,
                    OrderId: chargeRequest.OrderId,
                    Amount: chargeRequest.Amount,
                    Currency: chargeRequest.Currency,
                    ProviderTransactionId: providerTransactionId,
                    AttemptNumber: attemptNumber),
                cancellationToken);

            lastInquiryResponse = inquiryResponse;

            switch (inquiryResponse.State)
            {
                case ProviderPaymentState.Succeeded:
                    return ToSuccessOrStillProcessing(
                        inquiryResponse.ProviderTransactionId,
                        "PROVIDER_INQUIRY_TRANSACTION_ID_MISSING",
                        "Provider inquiry returned success but provider transaction id is missing.");

                case ProviderPaymentState.Failed:
                    return ProviderFinalResult.Failed(
                        inquiryResponse.ErrorCode ?? "PROVIDER_INQUIRY_FAILED",
                        inquiryResponse.ErrorMessage ?? "Provider inquiry returned failed.");

                case ProviderPaymentState.StillProcessing:
                    continue;

                default:
                    return ProviderFinalResult.StillProcessing(
                        "PROVIDER_INQUIRY_UNKNOWN_STATE",
                        $"Provider inquiry returned unknown state: {inquiryResponse.State}");
            }
        }

        return ProviderFinalResult.StillProcessing(
            lastInquiryResponse?.ErrorCode ?? "PROVIDER_INQUIRY_UNRESOLVED",
            lastInquiryResponse?.ErrorMessage ?? "Provider inquiry could not resolve payment status.");
    }

    private static async Task<ProviderInquiryResponse> InquirySafelyAsync(
        IPaymentProvider provider,
        ProviderInquiryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await provider.InquiryAsync(
                request,
                cancellationToken);
        }
        catch (TimeoutException ex)
        {
            return ProviderInquiryResponse.StillProcessing(
                errorCode: "PROVIDER_INQUIRY_TIMEOUT",
                errorMessage: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return ProviderInquiryResponse.StillProcessing(
                errorCode: "PROVIDER_INQUIRY_TIMEOUT",
                errorMessage: ex.Message);
        }
    }

    private static ProviderFinalResult ToSuccessOrStillProcessing(
        string? providerTransactionId,
        string missingTransactionErrorCode,
        string missingTransactionErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(providerTransactionId))
        {
            return ProviderFinalResult.StillProcessing(
                missingTransactionErrorCode,
                missingTransactionErrorMessage);
        }

        return ProviderFinalResult.Success(providerTransactionId);
    }
}