
namespace PayBridge.BuildingBlocks.Security.IntegrationTokens
{
    public sealed class IntegrationAuthOptions
    {
        public string Issuer { get; set; } = "PayBridge";
        public string Audience { get; set; } = "PayBridgeApi";
        public string SigningKey { get; set; } = default!;
        public int TokenTtlSeconds { get; set; } = 300;
        public List<IntegrationClientOptions> Clients { get; set; } = [];
    }
}
