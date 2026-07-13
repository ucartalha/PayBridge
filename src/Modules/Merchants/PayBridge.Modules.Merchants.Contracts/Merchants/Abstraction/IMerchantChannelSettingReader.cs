using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction
{
    public interface IMerchantChannelSettingReader
    {
        Task<MerchantChannelSettingInfo?> GetAsync(
            Guid merchantId,
            string channel,
            CancellationToken cancellationToken = default);
    }
}
