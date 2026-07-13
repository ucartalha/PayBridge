using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayBridge.BuildingBlocks.Persistence;
using PayBridge.Modules.Merchants.Application;
using PayBridge.Modules.Merchants.Application.Abstractions;
using PayBridge.Modules.Merchants.Contracts.Credentials;
using PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;
using PayBridge.Modules.Merchants.Infrastructure.Persistence;
using PayBridge.Modules.Merchants.Infrastructure.Persistence.Readers;
using PayBridge.Modules.Merchants.Infrastructure.Persistence.Repositories;
using PayBridge.Modules.Merchants.Infrastructure.Security;

namespace PayBridge.Modules.Merchants.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMerchantsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMerchantsApplication();

        var connectionString = configuration.GetConnectionString("MerchantsDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'MerchantsDatabase' was not found.");
        }

        services.AddDbContext<MerchantsDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
        services.AddDataProtection();

        services.AddSingleton<ICredentialProtector, DevelopmentCredentialProtector>();
        services.AddScoped<MerchantsUnitOfWork>();
        services.AddScoped<IMerchantsUnitOfWork, MerchantsUnitOfWork>();

        services.AddScoped<IUnitOfWorkAccessor, MerchantsUnitOfWorkAccessor>();

        services.AddScoped<IRepository<Merchant>, EfRepository<Merchant>>();
        services.AddScoped<IRepository<MerchantSector>, EfRepository<MerchantSector>>();
        services.AddScoped<IRepository<MerchantCategoryCode>, EfRepository<MerchantCategoryCode>>();
        services.AddScoped<IRepository<MerchantPaymentChannelSetting>, EfRepository<MerchantPaymentChannelSetting>>();
        services.AddScoped<IRepository<MerchantProviderAccount>, EfRepository<MerchantProviderAccount>>();
        services.AddScoped<IRepository<MerchantProviderCredential>, EfRepository<MerchantProviderCredential>>();

        services.AddScoped<IMerchantReader, MerchantReader>();
        services.AddScoped<IMerchantChannelSettingReader, MerchantChannelSettingReader>();
        services.AddScoped<IMerchantProviderAccountReader, MerchantProviderAccountReader>();
        return services;
    }
}