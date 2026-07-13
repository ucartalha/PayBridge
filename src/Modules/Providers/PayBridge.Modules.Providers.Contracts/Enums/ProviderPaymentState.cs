using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Providers.Contracts.Enums
{
    public enum ProviderPaymentState
    {
        Unknown =0,
        Succeeded = 1,
        Failed = 2,
        StillProcessing = 3,
        Cancelled = 4,
        Rejected = 5
    }
}
