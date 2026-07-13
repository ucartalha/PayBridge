using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Payments.PaymentsExecution
{
    public sealed record PaymentExecutionResult(
        Guid PaymentId,
        string Status,
        string? ProviderTransactionId,
        string? ErrorCode,
        string? ErrorMessage);
}
