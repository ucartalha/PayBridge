using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Providers.Contracts
{
    public interface IPaymentProvider
    {
        string ProviderCode { get; }

        Task<ProviderChargeResponse> ChargeAsync(
            ProviderChargeRequest request,
            CancellationToken cancellationToken = default);
        Task<ProviderInquiryResponse> InquiryAsync(
      ProviderInquiryRequest request,
      CancellationToken cancellationToken = default);
    }
}
