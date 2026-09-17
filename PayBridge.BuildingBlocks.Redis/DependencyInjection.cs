using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using PayBridge.BuildingBlocks.Redis.Idempotency;
using StackExchange.Redis;

namespace PayBridge.BuildingBlocks.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddRedisInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException(
                "Redis connection string was not found.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options =
                ConfigurationOptions.Parse(connectionString);

            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });

        services
            .AddOptions<RedisCacheOptions>()
            .Configure<IConnectionMultiplexer>(
                (options, multiplexer) =>
                {
                    options.ConnectionMultiplexerFactory =
                        () => Task.FromResult(multiplexer);
                });

        services.AddSingleton<
            IIdempotencyGate,
            RedisIdempotencyGate>();

        return services;
    }
}