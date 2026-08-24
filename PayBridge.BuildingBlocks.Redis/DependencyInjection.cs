using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using PayBridge.BuildingBlocks.Redis.Idempotency;
using StackExchange.Redis;

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

        services.AddSingleton<
            IIdempotencyGate,
            RedisIdempotencyGate>();

        return services;
    }
}