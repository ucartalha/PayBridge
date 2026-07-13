using PayBridge.Modules.Payments.Domain.Payments.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Abstractions
{
    public interface IPaymentTransactionRepository
    {
        Task AddAsync(PaymentTransaction paymentTransaction, CancellationToken cancellationToken = default);
    }
}
