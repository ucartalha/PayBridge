using PayBridge.Modules.Providers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Abstractions
{
    public interface IProviderCredentialResolver
    {
        Task<ProviderCredentialContext> ResolveAsync(Guid merchantId, string providerCode,string channel, CancellationToken cancellationToken);
    }
}
