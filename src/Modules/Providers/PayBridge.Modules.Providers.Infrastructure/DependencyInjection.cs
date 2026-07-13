using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayBridge.Modules.Providers.Contracts;
using PayBridge.Modules.Providers.Infrastructure.Mock;

namespace PayBridge.Modules.Providers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProvidersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPaymentProvider, MockPaymentProvider>();
        services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();

        return services;
    }
}