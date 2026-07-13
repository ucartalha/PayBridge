using Microsoft.EntityFrameworkCore;
using PayBridge.Modules.Merchants.Contracts.Merchants;
using PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Readers
{
    internal sealed class MerchantReader : IMerchantReader
    {
        private readonly MerchantsDbContext _dbContext;

        public MerchantReader(MerchantsDbContext context)
        {
            _dbContext = context;
        }
        public Task<MerchantPaymentAccessInfo?> GetPaymentAccessInfoByCodeAsync(string merchantCode, string channel, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(merchantCode))
            {
                return Task.FromResult<MerchantPaymentAccessInfo?>(null);
            }

            return GetPaymentAccessInfoAsync(
                merchantId : null,
                merchantCode.Trim(),
                channel,
                cancellationToken);
        }

        public Task<MerchantPaymentAccessInfo?> GetPaymentAccessInfoByIdAsync(Guid merchantId, string channel, CancellationToken cancellationToken = default)
        {
            if (merchantId == Guid.Empty)
            {
                return Task.FromResult<MerchantPaymentAccessInfo?>(null);
            }

            return GetPaymentAccessInfoAsync(
                merchantId,
                merchantCode: null,
                channel,
                cancellationToken);
        }

        private async Task<MerchantPaymentAccessInfo?> GetPaymentAccessInfoAsync(
            Guid? merchantId,
            string? merchantCode,
            string channel,
            CancellationToken cancellationToken)
        {
            if (!MerchantChannelMapper.TryParse(channel, out var parsedChannel))
            {
                return null;
            }

            var query = from merchant in _dbContext.Merchants.AsNoTracking()
                        join sector in _dbContext.MerchantSectors.AsNoTracking()
                            on merchant.SectorId equals sector.Id
                        join mcc in _dbContext.MerchantCategoryCodes.AsNoTracking()
                            on merchant.MccId equals mcc.Id
                        join channelSetting in _dbContext.MerchantPaymentChannelSettings.AsNoTracking()
                            on merchant.Id equals channelSetting.MerchantId
                        where channelSetting.Channel == parsedChannel
                        select new
                        {
                            MerchantId = merchant.Id,
                            merchant.MerchantCode,
                            MerchantDisplayName = merchant.DisplayName,
                            MerchantStatus = merchant.Status,

                            SectorId = sector.Id,
                            SectorCode = sector.Code,
                            SectorName = sector.Name,
                            SectorIsHighRisk = sector.IsHighRisk,
                            SectorIsActive = sector.IsActive,

                            MccId = mcc.Id,
                            MccCode = mcc.Code,
                            MccDescription = mcc.Description,
                            MccIsRestricted = mcc.IsRestricted,
                            MccIsActive = mcc.IsActive,

                            Channel = channelSetting.Channel,
                            ChannelIsEnabled = channelSetting.IsEnabled,
                            channelSetting.MinAmount,
                            channelSetting.MaxAmount,
                            channelSetting.DailyAmountLimit,
                            channelSetting.Require3DS,
                            channelSetting.AllowRefund,
                            channelSetting.AllowVoid
                        };
            if (merchantId.HasValue)
            {
                query = query.Where(x => x.MerchantId == merchantId.Value);
            }
            if (!string.IsNullOrWhiteSpace(merchantCode))
            {
                query = query.Where(x => x.MerchantCode == merchantCode);
            }
            var result = await query.FirstOrDefaultAsync(cancellationToken);

            if (result is null)
            {
                return null;
            }

            return new MerchantPaymentAccessInfo(
                result.MerchantId,
                result.MerchantCode,
                result.MerchantDisplayName,
                result.MerchantStatus.ToString(),
                result.SectorId,
                result.SectorCode,
                result.SectorName,
                result.SectorIsHighRisk,
                result.SectorIsActive,
                result.MccId,
                result.MccCode,
                result.MccDescription,
                result.MccIsRestricted,
                result.MccIsActive,
                result.Channel.ToString(),
                result.ChannelIsEnabled,
                result.MinAmount.Value,
                result.MaxAmount.Value,
                result.DailyAmountLimit,
                result.Require3DS,
                result.AllowRefund,
                result.AllowVoid
                );
        }
    }
}
