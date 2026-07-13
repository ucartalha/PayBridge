using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Providers.Contracts;
using PayBridge.Modules.Providers.Contracts.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Providers.Infrastructure
{
    internal sealed class PaymentProviderFactory : IPaymentProviderFactory
    {
        private readonly IEnumerable<IPaymentProvider> _providers;
        public PaymentProviderFactory(IEnumerable<IPaymentProvider> providers)
        {
            _providers = providers;
        }
        public IPaymentProvider Resolve(string providerCode)
        {
            var provider = _providers.FirstOrDefault(
                p => string.Equals(p.ProviderCode, 
                providerCode, 
                StringComparison.OrdinalIgnoreCase));

            if (provider is null)
            {
                throw new BusinessException((int)ProviderErrorCode.ProviderNotSupported);
            }

            return provider;
        }
    }
}
