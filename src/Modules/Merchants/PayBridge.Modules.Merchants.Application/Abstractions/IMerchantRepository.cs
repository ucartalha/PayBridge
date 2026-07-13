using PayBridge.Modules.Merchants.Domain.Merchants.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Application.Abstractions
{
    public interface IMerchantRepository
    {
        Task<Merchant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Merchant?> GetByMerchantKeyAsync(
            string merchantKey,
            CancellationToken cancellationToken = default);

        Task AddAsync(Merchant merchant, CancellationToken cancellationToken = default);
    }
}
