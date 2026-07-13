using PayBridge.Modules.Payments.Application.Payments.CreatePayment;
using PayBridge.Modules.Payments.Application.Payments.PaymentsExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Abstractions
{
    public interface IPaymentOrchestrator
    {
        Task<PaymentExecutionResult> ExecutePaymentAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default);
    }
}
