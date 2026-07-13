using PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;
using PayBridge.Modules.Providers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Abstractions
{
    public interface IProviderPaymentResultResolver
    {
        Task<ProviderFinalResult> ResolveAsync(
            IPaymentProvider provider,
            ProviderChargeRequest chargeRequest,
            CancellationToken cancellationToken = default);
    }
}
