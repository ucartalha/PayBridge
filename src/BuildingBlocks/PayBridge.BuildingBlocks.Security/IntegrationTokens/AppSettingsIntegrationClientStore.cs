using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace PayBridge.BuildingBlocks.Security.IntegrationTokens;

public sealed class AppSettingsIntegrationClientStore : IIntegrationClientStore
{
    private readonly IntegrationAuthOptions _options;

    public AppSettingsIntegrationClientStore(IOptions<IntegrationAuthOptions> options)
    {
        _options = options.Value;
    }

    public Task<IntegrationClient?> ValidateAsync(
        string clientCode,
        string clientSecret,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientCode) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            return Task.FromResult<IntegrationClient?>(null);
        }

        var client = _options.Clients.FirstOrDefault(client =>
            client.IsActive &&
            string.Equals(
                client.ClientCode,
                clientCode,
                StringComparison.OrdinalIgnoreCase));

        if (client is null)
        {
            return Task.FromResult<IntegrationClient?>(null);
        }

        if (!SecretEquals(client.ClientSecret, clientSecret))
        {
            return Task.FromResult<IntegrationClient?>(null);
        }

        var integrationClient = new IntegrationClient(
            Id: client.Id,
            ClientCode: client.ClientCode,
            Scopes: client.Scopes ?? []);

        return Task.FromResult<IntegrationClient?>(integrationClient);
    }

    private static bool SecretEquals(string expected, string actual)
    {
        var expectedBytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(expected));

        var actualBytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(actual));

        return CryptographicOperations.FixedTimeEquals(
            expectedBytes,
            actualBytes);
    }
}