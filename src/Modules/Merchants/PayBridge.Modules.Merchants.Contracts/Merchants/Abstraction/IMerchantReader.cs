using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction
{
    public interface IMerchantReader
    {
        Task<MerchantPaymentAccessInfo?> GetPaymentAccessInfoByIdAsync(
            Guid merchantId,
            string channel, 
            CancellationToken cancellationToken = default);

        Task<MerchantPaymentAccessInfo?> GetPaymentAccessInfoByCodeAsync(
            string merchantCode,
            string channel,
            CancellationToken cancellationToken = default);
    }
}
