using PayBridge.BuildingBlocks.CQRS;
using PayBridge.BuildingBlocks.Redis;
using PayBridge.Modules.Merchants.Infrastructure;
using PayBridge.Modules.Payments.Infrastructure;
using PayBridge.Modules.Providers.Infrastructure;

namespace PayBridge.Api.Modules
{
    public static class ModulesBootstrapper
    {
        public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddRedisInfrastructure(configuration);
            
            services.AddMerchantsModule(configuration);
            services.AddPaymentsModule(configuration);
            services.AddProvidersModule(configuration);

            services.AddCQRS();

            return services;
        }
    }
}
