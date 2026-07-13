using Microsoft.EntityFrameworkCore;
using PayBridge.Modules.Merchants.Contracts.Merchants;
using PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction;
using PayBridge.Modules.Merchants.Domain.Merchants.Enums;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Readers;

internal sealed class MerchantProviderAccountReader : IMerchantProviderAccountReader
{
    private readonly MerchantsDbContext _dbContext;

    public MerchantProviderAccountReader(MerchantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MerchantProviderAccountInfo?> GetActiveProviderAccountAsync(
        Guid merchantId,
        string providerCode,
        string channel,
        CancellationToken cancellationToken = default)
    {
        if (merchantId == Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(providerCode))
        {
            return null;
        }

        if (!MerchantChannelMapper.TryParse(channel, out var parsedChannel))
        {
            return null;
        }

        var normalizedProviderCode = providerCode.Trim();

        var query = _dbContext.MerchantProviderAccounts
            .AsNoTracking()
            .Where(x =>
                x.MerchantId == merchantId &&
                x.ProviderCode == normalizedProviderCode &&
                x.IsActive);

        query = parsedChannel switch
        {
            MerchantPaymentChannel.ECommerce =>
                query.Where(x => x.AllowECommerce),

            MerchantPaymentChannel.PhysicalPos =>
                query.Where(x => x.AllowPhysicalPos),

            MerchantPaymentChannel.Wallet =>
                query.Where(x => false),

            _ =>
                query.Where(x => false)
        };

        return await query
            .OrderBy(x => x.Priority)
            .Select(x => new MerchantProviderAccountInfo(
                x.Id,
                x.MerchantId,
                x.ProviderCode,
                x.IsActive,
                x.AllowECommerce,
                x.AllowPhysicalPos,
                x.AllowRefund,
                x.Priority))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MerchantProviderCredentialInfo?> GetActiveCredentialAsync(
        Guid merchantProviderAccountId,
        CancellationToken cancellationToken = default)
    {
        if (merchantProviderAccountId == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.MerchantProviderCredentials
            .AsNoTracking()
            .Where(x =>
                x.MerchantProviderAccountId == merchantProviderAccountId &&
                x.IsActive)
            .Select(x => new MerchantProviderCredentialInfo(
                x.Id,
                x.MerchantProviderAccountId,
                x.EncryptedCredentialPayload,
                x.EncryptedKeyVersion,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}