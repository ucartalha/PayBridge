using PayBridge.Modules.Merchants.Contracts.Merchants;
using PayBridge.Modules.Merchants.Domain.Merchants.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Infrastructure.Persistence.Readers
{
    internal static class MerchantChannelMapper
    {
        public static bool TryParse(string channel, out MerchantPaymentChannel merchantPaymentChannel)
        {
            merchantPaymentChannel = default;

            if (string.IsNullOrWhiteSpace(channel))
            {
                return false;
            }
            var normalizedChannel = MerchantPaymentChannels.NormalizeChannel(channel.Trim());

            return Enum.TryParse(normalizedChannel,
                ignoreCase: true,
                out merchantPaymentChannel);
        }
    }
}
