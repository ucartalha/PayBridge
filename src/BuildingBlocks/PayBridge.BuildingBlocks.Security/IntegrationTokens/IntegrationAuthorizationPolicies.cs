using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Security.IntegrationTokens
{
    public static class IntegrationAuthorizationPolicies
    {
        public const string PaymentsCreate = "payments:create";
        public const string PaymentsRefund = "payments:refund";
        public const string PaymentsStatus = "payments:status";
    }
}
