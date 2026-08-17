using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayBridge.Modules.Providers.Contracts;
using PayBridge.Modules.Providers.Infrastructure.Decorators;
using PayBridge.Modules.Providers.Infrastructure.Mock;

namespace PayBridge.Modules.Providers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProvidersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<MockPaymentProvider>();

        services.AddScoped<IPaymentProvider>(serviceProvider =>
        {
            var innerProvider =
                serviceProvider.GetRequiredService<MockPaymentProvider>();

            var logger =
                serviceProvider.GetRequiredService<
                    ILogger<LoggingPaymentProviderDecorator>>();

            return new LoggingPaymentProviderDecorator(
                innerProvider,
                logger);
        });

        services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();

        return services;
    }
}