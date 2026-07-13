namespace PayBridge.BuildingBlocks.Security.IntegrationTokens;

public sealed class IntegrationClientOptions
{
    public Guid Id { get; set; }

    public string ClientCode { get; set; } = default!;

    public string ClientSecret { get; set; } = default!;

    public bool IsActive { get; set; }

    public List<string> Scopes { get; set; } = [];
}