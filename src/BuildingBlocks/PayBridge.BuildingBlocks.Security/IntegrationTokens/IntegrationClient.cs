namespace PayBridge.BuildingBlocks.Security.IntegrationTokens;

    public sealed record IntegrationClient(
        Guid Id,
    string ClientCode,
    IReadOnlyCollection<string> Scopes);

