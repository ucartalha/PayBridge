namespace PayBridge.BuildingBlocks.Security.IntegrationTokens;

public interface IIntegrationPaymentAccessService
{
    Task<bool> CanCreatePaymentAsync(
        Guid integrationClientId,
        Guid merchantId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
}