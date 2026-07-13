using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PayBridge.BuildingBlocks.CQRS.Behaviors;
using PayBridge.BuildingBlocks.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.CQRS
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCQRS(this IServiceCollection services) 
        {
            var applicationAssemblies = ApplicationAssemblyDiscovery.DiscoverApplicationAssemblies();

            return services.AddCQRS(applicationAssemblies);
        }

        public static IServiceCollection AddCQRS(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length ==0)
            {
                throw new InvalidOperationException("At least one application assembly must be provided for CQRS registeration.");
            }
            services.AddScoped<IUnitOfWorkResolver, DefaultUnitOfWorkResolver>();

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssemblies(assemblies);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(IdempotencyBehavior<,>));
                cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
                
            });

            services.AddValidatorsFromAssemblies(assemblies);
            return services;
        }
    }
}
