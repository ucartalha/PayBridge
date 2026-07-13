using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Providers.Contracts
{
    public interface IPaymentProviderFactory
    {
        IPaymentProvider Resolve(string providerCode);
    }
}
