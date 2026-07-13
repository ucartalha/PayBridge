using PayBridge.BuildingBlocks.Security.IntegrationTokens;

namespace PayBridge.Api.Authorization;

public sealed class AllowAllIntegrationPaymentAccessService : IIntegrationPaymentAccessService
{
    public Task<bool> CanCreatePaymentAsync(
        Guid integrationClientId,
        Guid merchantId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}