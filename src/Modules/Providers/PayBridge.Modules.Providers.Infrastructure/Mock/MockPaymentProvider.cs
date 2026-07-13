using PayBridge.Modules.Providers.Contracts;

namespace PayBridge.Modules.Providers.Infrastructure.Mock;

public sealed class MockPaymentProvider : IPaymentProvider
{
    public string ProviderCode => "MockBank";

    public Task<ProviderChargeResponse> ChargeAsync(
        ProviderChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount == 13)
        {
            return Task.FromResult(
                ProviderChargeResponse.Failed(
                    errorCode: "MOCK_PAYMENT_FAILED",
                    errorMessage: "Mock provider rejected the payment."));
        }

        if (request.Amount == 99)
        {
            return Task.FromResult(
                ProviderChargeResponse.InquiryRequired(
                    errorCode: "MOCK_PROVIDER_REQUIRES_INQUIRY",
                    errorMessage: "Mock provider response is uncertain."));
        }

        if (request.Amount == 98)
        {
            return Task.FromResult(
                ProviderChargeResponse.InquiryRequired(
                    errorCode: "MOCK_PROVIDER_TIMEOUT",
                    errorMessage: "Mock provider timeout occurred."));
        }

        if (request.Amount == 97)
        {
            return Task.FromResult(
                ProviderChargeResponse.InquiryRequired(
                    errorCode: "MOCK_PROVIDER_ALWAYS_UNKNOWN",
                    errorMessage: "Mock provider will not resolve this payment."));
        }

        return Task.FromResult(
            ProviderChargeResponse.Success(
                providerTransactionId: $"MOCK-{Guid.NewGuid():N}"));
    }

    public Task<ProviderInquiryResponse> InquiryAsync(
        ProviderInquiryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount == 99)
        {
            if (request.AttemptNumber == 1)
            {
                return Task.FromResult(
                    ProviderInquiryResponse.StillProcessing(
                        errorCode: "MOCK_INQUIRY_STILL_PROCESSING",
                        errorMessage: "Mock provider payment is still processing."));
            }

            return Task.FromResult(
                ProviderInquiryResponse.Success(
                    providerTransactionId: $"MOCK-INQ-{Guid.NewGuid():N}"));
        }

        if (request.Amount == 98)
        {
            if (request.AttemptNumber <= 2)
            {
                return Task.FromResult(
                    ProviderInquiryResponse.StillProcessing(
                        errorCode: "MOCK_INQUIRY_TIMEOUT",
                        errorMessage: "Mock provider inquiry timed out."));
            }

            return Task.FromResult(
                ProviderInquiryResponse.Failed(
                    errorCode: "MOCK_INQUIRY_FAILED_AFTER_TIMEOUT",
                    errorMessage: "Mock provider could not approve payment after timeout."));
        }

        if (request.Amount == 97)
        {
            return Task.FromResult(
                ProviderInquiryResponse.StillProcessing(
                    errorCode: "MOCK_INQUIRY_UNRESOLVED",
                    errorMessage: "Mock provider status is still unknown."));
        }

        if (request.Amount == 13)
        {
            return Task.FromResult(
                ProviderInquiryResponse.Failed(
                    errorCode: "MOCK_PAYMENT_FAILED",
                    errorMessage: "Mock provider rejected the payment."));
        }

        return Task.FromResult(
            ProviderInquiryResponse.Success(
                providerTransactionId: request.ProviderTransactionId
                    ?? $"MOCK-INQ-{Guid.NewGuid():N}"));
    }
}