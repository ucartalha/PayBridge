using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Payments.CreatePayment
{
    public sealed record CreatePaymentResult(Guid PaymentId, string Status);
}
