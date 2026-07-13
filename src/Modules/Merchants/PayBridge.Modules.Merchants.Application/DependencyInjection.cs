using Microsoft.Extensions.DependencyInjection;

namespace PayBridge.Modules.Merchants.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMerchantsApplication(
        this IServiceCollection services)
    {
        return services;
    }
}