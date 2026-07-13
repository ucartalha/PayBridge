using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Security.IntegrationTokens
{
    public sealed record IssueIntegrationTokenRequest(
        string ClientCode,
        string ClientSecret);

    public sealed record IssueIntegrationTokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTime ExpiresAtUtc);

}
