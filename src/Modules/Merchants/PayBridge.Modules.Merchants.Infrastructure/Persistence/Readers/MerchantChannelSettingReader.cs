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
    internal sealed class MerchantChannelSettingReader : IMerchantChannelSettingReader
    {
        private readonly MerchantsDbContext _dbContext;
        public MerchantChannelSettingReader(MerchantsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<MerchantChannelSettingInfo?> GetAsync(Guid merchantId, string channel, CancellationToken cancellationToken = default)
        {
            if (merchantId == Guid.Empty)
            {
                return null;
            }
            if (!MerchantChannelMapper.TryParse(channel, out var parsedChannel)) 
            {
                return null;
            }
            return await _dbContext.MerchantPaymentChannelSettings.AsNoTracking()
                .Where(x=> x.MerchantId == merchantId && x.Channel == parsedChannel)
                .Select(x=> new MerchantChannelSettingInfo(
                    x.Id,
                    x.MerchantId,
                    x.Channel.ToString(),
                    x.IsEnabled,
                    x.MinAmount.Value,
                    x.MaxAmount.Value,
                    x.DailyAmountLimit,
                    x.Require3DS,
                    x.AllowRefund,
                    x.AllowVoid)).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
