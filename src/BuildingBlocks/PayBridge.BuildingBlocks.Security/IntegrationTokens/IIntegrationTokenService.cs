using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Security.IntegrationTokens
{
    public interface IIntegrationTokenService
    {
        Task<IssueIntegrationTokenResponse> IssueAsync(
            IntegrationClient client,
            CancellationToken cancellationToken = default);
        Task<bool> IsActiveAsync(
            string jti,
            CancellationToken cancellationToken = default);
    }
}
