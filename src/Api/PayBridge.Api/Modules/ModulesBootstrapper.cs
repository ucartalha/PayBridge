using PayBridge.BuildingBlocks.CQRS;
using PayBridge.Modules.Merchants.Infrastructure;
using PayBridge.Modules.Payments.Infrastructure;
using PayBridge.Modules.Providers.Infrastructure;

namespace PayBridge.Api.Modules
{
    public static class ModulesBootstrapper
    {
        public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCQRS();

            services.AddMerchantsModule(configuration);
            services.AddPaymentsModule(configuration);
            services.AddProvidersModule(configuration);

            return services;
        }
    }
}
