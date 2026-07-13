using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Payments.CompletePayment
{
    public sealed record CompletePaymentResult(
        Guid PaymentId,
        string Status,
        string? ProviderTransactionId,
        string? ErrorCode,
        string? ErrorMessage);

}
