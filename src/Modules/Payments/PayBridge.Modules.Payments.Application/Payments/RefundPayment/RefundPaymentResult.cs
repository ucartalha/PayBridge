using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Payments.RefundPayment
{
    public sealed record RefundPaymentResult(Guid PaymentId, string ProviderTxId, decimal RefundedAmount, decimal RefundableAmount);
}
