using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.BuildingBlocks.Persistence.Idempotency;
using PayBridge.Modules.Payments.Application;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Payments.Domain.Payments.Entities;
using PayBridge.Modules.Payments.Infrastructure.PaymentsExecution;
using PayBridge.Modules.Payments.Infrastructure.Persistence;
using PayBridge.Modules.Payments.Infrastructure.Persistence.Idempotency;
using PayBridge.Modules.Payments.Infrastructure.Persistence.Repositories;

namespace PayBridge.Modules.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddPaymentsApplication();

        var connectionString = configuration.GetConnectionString("PaymentsDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'PaymentsDatabase' was not found.");
        }

        services.AddDbContext<PaymentsDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<PaymentsUnitOfWork>();

        services.AddScoped<IUnitOfWorkAccessor, PaymentsUnitOfWorkAccessor>();

        services.AddScoped<IRepository<Payment>, EfRepository<Payment>>();
        services.AddScoped<IRepository<PaymentTransaction>, EfRepository<PaymentTransaction>>();
        services.AddScoped<IRepository<IdempotencyRecord>, EfRepository<IdempotencyRecord>>();

        
        services.AddScoped<IIdempotencyService, PaymentsIdempotencyService>();
        services.AddScoped<IProviderCredentialResolver, ProviderCredentialResolver>();

        return services;
    }
}