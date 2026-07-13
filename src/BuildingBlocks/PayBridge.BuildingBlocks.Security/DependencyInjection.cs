using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PayBridge.BuildingBlocks.Security.IntegrationTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PayBridge.BuildingBlocks.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrationJwtSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var integrationAuthOptions = configuration
            .GetSection("IntegrationAuth")
            .Get<IntegrationAuthOptions>();

        if (integrationAuthOptions is null)
        {
            throw new InvalidOperationException(
                "IntegrationAuth configuration was not found.");
        }

        if (string.IsNullOrWhiteSpace(integrationAuthOptions.SigningKey))
        {
            throw new InvalidOperationException(
                "IntegrationAuth SigningKey was not found.");
        }

        if (integrationAuthOptions.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "IntegrationAuth SigningKey must be at least 32 characters.");
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        services.Configure<IntegrationAuthOptions>(
            configuration.GetSection("IntegrationAuth"));

        services.AddScoped<IIntegrationClientStore, AppSettingsIntegrationClientStore>();
        services.AddScoped<IIntegrationTokenService, JwtIntegrationTokenService>();

        services
            .AddAuthentication(IntegrationJwtDefaults.Scheme)
            .AddJwtBearer(IntegrationJwtDefaults.Scheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = integrationAuthOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = integrationAuthOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(integrationAuthOptions.SigningKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jti = context.Principal?
                            .FindFirst(JwtRegisteredClaimNames.Jti)?
                            .Value;

                        var clientCode = context.Principal?
                            .FindFirst(IntegrationAuthClaimNames.ClientCode)?
                            .Value;

                        if (string.IsNullOrWhiteSpace(jti) ||
                            string.IsNullOrWhiteSpace(clientCode))
                        {
                            context.Fail("Required integration token claims are missing.");
                            return;
                        }

                        var tokenService = context.HttpContext.RequestServices
                            .GetRequiredService<IIntegrationTokenService>();

                        var isActive = await tokenService.IsActiveAsync(
                            jti,
                            context.HttpContext.RequestAborted);

                        if (!isActive)
                        {
                            context.Fail("Integration token is not active.");
                        }
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}