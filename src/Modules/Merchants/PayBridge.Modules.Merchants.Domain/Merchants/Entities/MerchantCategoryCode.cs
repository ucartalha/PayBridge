using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Merchants.Domain.Merchants.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Domain.Merchants.Entities
{
    public sealed class MerchantCategoryCode
    {
        public Guid Id { get; private set; }

        public string Code { get; private set; } = default!;
        public string Description { get; private set; } = default!;

        public bool IsRestricted { get; private set; }
        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? DeactivatedAtUtc { get; private set; }

        private MerchantCategoryCode()
        {
        }

        public static MerchantCategoryCode Create(
            string code,
            string description,
            bool isRestricted = false)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new BusinessException((int)MerchantErrorCode.MccCodeRequired);
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new BusinessException((int)MerchantErrorCode.MccDescriptionRequired);
            }
            return new MerchantCategoryCode
            {
                Id = Guid.NewGuid(),
                Code = code.Trim(),
                Description = description.Trim(),
                IsRestricted = isRestricted,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };   
        }

        public void Activate()
        {
            IsActive = true;
            DeactivatedAtUtc = null;
        }
        public void Deactivate()
        {
            IsActive = false;
            DeactivatedAtUtc = DateTime.UtcNow;
        }
        public void RemoveRestriction()
        {
            IsRestricted = false;
        }
        public void EnsureCanBeUsedForMerchant()
        {
            if (!IsActive)
            {
                throw new BusinessException((int)MerchantErrorCode.MccNotActive);
            }
            if (IsRestricted)
            {
                throw new BusinessException((int)MerchantErrorCode.MccRestricted);
            }
        }
    }
}
