using PayBridge.Modules.Payments.Domain.Payments.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Application.Abstractions
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);
        Task<Payment?> GetByMerchantAndOrderId(Guid merchantId, Guid orderId, CancellationToken cancellationToken = default);
        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
