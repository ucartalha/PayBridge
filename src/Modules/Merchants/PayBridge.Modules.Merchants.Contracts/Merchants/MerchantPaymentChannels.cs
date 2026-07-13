using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Contracts.Merchants
{
    public static class MerchantPaymentChannels
    {
        public const string ECommerce = "ECommerce";
        public const string PhysicalPos = "PhysicalPos";
        public const string Wallet = "Wallet";

        public static bool IsSupportedChannel(string channel)
        {
            return string.Equals(channel, ECommerce, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(channel, PhysicalPos, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(channel, Wallet, StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeChannel(string channel)
        {
            if (string.Equals(channel, ECommerce, StringComparison.OrdinalIgnoreCase))
            {
                return ECommerce;
            }
            if (string.Equals(channel, PhysicalPos, StringComparison.OrdinalIgnoreCase))
            {
                return PhysicalPos;
            }
            if (string.Equals(channel, Wallet, StringComparison.OrdinalIgnoreCase))
            {
                return Wallet;
            }
            return channel;
        }
    }
}
