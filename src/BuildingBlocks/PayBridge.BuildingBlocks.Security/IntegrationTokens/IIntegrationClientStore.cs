using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Security.IntegrationTokens
{
    public interface IIntegrationClientStore
    {
        Task<IntegrationClient?> ValidateAsync(
            string clientCode,
            string clientSecret,
            CancellationToken cancellationToken = default);
    }
}
