using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PayBridge.BuildingBlocks.Security.IntegrationTokens;

public sealed class JwtIntegrationTokenService : IIntegrationTokenService
{
    private const string TokenType = "Bearer";
    private const int MaxTokenTtlSeconds = 300;

    private readonly IDistributedCache _cache;
    private readonly IntegrationAuthOptions _options;

    public JwtIntegrationTokenService(
        IDistributedCache cache,
        IOptions<IntegrationAuthOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task<bool> IsActiveAsync(
        string jti,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return false;
        }

        var cacheKey = GetCacheKey(jti);

        var value = await _cache.GetStringAsync(
            cacheKey,
            cancellationToken);

        return !string.IsNullOrWhiteSpace(value);
    }

    public async Task<IssueIntegrationTokenResponse> IssueAsync(
        IntegrationClient client,
        CancellationToken cancellationToken = default)
    {
        var ttlSeconds = Math.Clamp(
            _options.TokenTtlSeconds,
            1,
            MaxTokenTtlSeconds);

        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddSeconds(ttlSeconds);

        var jti = GenerateJti();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.ClientCode),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(
                JwtRegisteredClaimNames.Iat,
                ToUnixTimeSeconds(issuedAtUtc).ToString(),
                ClaimValueTypes.Integer64),

            new(
                IntegrationAuthClaimNames.IntegrationClientId,
                client.Id.ToString()),

            new(
                IntegrationAuthClaimNames.ClientCode,
                client.ClientCode)
        };

        foreach (var scope in client.Scopes)
        {
            claims.Add(new Claim(
                IntegrationAuthClaimNames.Scope,
                scope));
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(jwt);

        await StoreJtiAsync(
            jti,
            client,
            ttlSeconds,
            cancellationToken);

        return new IssueIntegrationTokenResponse(
            AccessToken: accessToken,
            TokenType: TokenType,
            ExpiresIn: ttlSeconds,
            ExpiresAtUtc: expiresAtUtc);
    }

    private static string GenerateJti()
    {
        return Convert
            .ToHexString(RandomNumberGenerator.GetBytes(16))
            .ToLowerInvariant();
    }

    private static long ToUnixTimeSeconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime)
            .ToUnixTimeSeconds();
    }

    private static string GetCacheKey(string jti)
    {
        return $"paybridge:integration-token:jti:{jti}";
    }

    private async Task StoreJtiAsync(
        string jti,
        IntegrationClient client,
        int ttlSeconds,
        CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(jti);

        var value = $"{client.Id}:{client.ClientCode}";

        await _cache.SetStringAsync(
            cacheKey,
            value,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds)
            },
            cancellationToken);
    }
}