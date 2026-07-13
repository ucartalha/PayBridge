using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayBridge.Modules.Merchants.Contracts.Credentials;
using PayBridge.Modules.Merchants.Domain.Merchants.Entities;
using PayBridge.Modules.Merchants.Domain.Merchants.Enums;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Seed;

public static class MerchantMockDataSeeder
{
    private const string MockMerchantCode = "MOCK-MERCHANT";
    private const string MockProviderCode = "Mock";
    private const string EncryptionKeyVersion = "dev-v1";

    public static async Task SeedMerchantMockDataAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MerchantsDbContext>();
        var credentialProtector = scope.ServiceProvider.GetRequiredService<ICredentialProtector>();

        var sector = await dbContext.MerchantSectors
            .FirstOrDefaultAsync(x => x.Code == "ECOMMERCE", cancellationToken);

        if (sector is null)
        {
            sector = MerchantSector.Create(
                code: "ECOMMERCE",
                name: "E-Commerce",
                isHighRisk: false);

            await dbContext.MerchantSectors.AddAsync(sector, cancellationToken);
        }

        var mcc = await dbContext.MerchantCategoryCodes
            .FirstOrDefaultAsync(x => x.Code == "5999", cancellationToken);

        if (mcc is null)
        {
            mcc = MerchantCategoryCode.Create(
                code: "5999",
                description: "Miscellaneous and Specialty Retail Stores",
                isRestricted: false);

            await dbContext.MerchantCategoryCodes.AddAsync(mcc, cancellationToken);
        }

        var merchant = await dbContext.Merchants
            .FirstOrDefaultAsync(x => x.MerchantCode == MockMerchantCode, cancellationToken);

        if (merchant is null)
        {
            merchant = Merchant.Create(
                merchantCode: MockMerchantCode,
                legalName: "Mock Merchant A.Ş.",
                displayName: "Mock Merchant",
                taxNumber: "1111111111",
                taxOffice: "Mock Vergi Dairesi",
                sectorId: sector.Id,
                mccId: mcc.Id);

            merchant.Activate();

            await dbContext.Merchants.AddAsync(merchant, cancellationToken);
        }

        var channelSetting = await dbContext.MerchantPaymentChannelSettings
            .FirstOrDefaultAsync(
                x => x.MerchantId == merchant.Id &&
                     x.Channel == MerchantPaymentChannel.ECommerce,
                cancellationToken);

        if (channelSetting is null)
        {
            channelSetting = MerchantPaymentChannelSetting.Create(
                merchantId: merchant.Id,
                channel: MerchantPaymentChannel.ECommerce,
                minAmount: 1,
                maxAmount: 100_000,
                dailyAmountLimit: 1_000_000,
                require3DS: false,
                allowRefund: true,
                allowVoid: true);

            channelSetting.Enable();

            await dbContext.MerchantPaymentChannelSettings.AddAsync(
                channelSetting,
                cancellationToken);
        }

        var providerAccount = await dbContext.MerchantProviderAccounts
            .FirstOrDefaultAsync(
                x => x.MerchantId == merchant.Id &&
                     x.ProviderCode == MockProviderCode,
                cancellationToken);

        if (providerAccount is null)
        {
            providerAccount = MerchantProviderAccount.Create(
             merchantId: merchant.Id,
             providerCode: MockProviderCode,
             priority: 1);

            providerAccount.ConfigureChannels(
                allowECommerce: true,
                allowPhysicalPos: false);

            providerAccount.ConfigureRefund(
                allowRefund: true);

            providerAccount.Activate();

            await dbContext.MerchantProviderAccounts.AddAsync(
                providerAccount,
                cancellationToken);
        }

        var hasActiveCredential = await dbContext.MerchantProviderCredentials
            .AnyAsync(
                x => x.MerchantProviderAccountId == providerAccount.Id &&
                     x.IsActive,
                cancellationToken);

        if (!hasActiveCredential)
        {
            var credentialPayload = new
            {
                providerCode = MockProviderCode,
                merchantNumber = "MOCK-MERCHANT-NO",
                terminalNumber = "MOCK-TERMINAL-NO",
                apiUser = "mock-api-user",
                apiPassword = "mock-api-password",
                baseUrl = "https://mock-provider.local"
            };

            var credentialPayloadJson = JsonSerializer.Serialize(credentialPayload);

            var encryptedCredentialPayload = credentialProtector.Protect(
                credentialPayloadJson,
                EncryptionKeyVersion);

            var credential = MerchantProviderCredential.Create(
                merchantProviderAccountId: providerAccount.Id,
                encryptedCredentialPayload: encryptedCredentialPayload,
                encryptedKeyVersion: EncryptionKeyVersion);

            await dbContext.MerchantProviderCredentials.AddAsync(
                credential,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}