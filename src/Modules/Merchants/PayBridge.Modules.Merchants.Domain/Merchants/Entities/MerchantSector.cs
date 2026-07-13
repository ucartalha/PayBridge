using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Merchants.Domain.Merchants.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Domain.Merchants.Entities
{
    public sealed class MerchantSector
    {
        public Guid Id{ get; private set; }

        public string Code { get; private set; } = default!;
        public string Name { get; private set; } = default!;

        public bool IsHighRisk{ get; private set; }
        public bool IsActive{ get;private set; }

        public DateTime CreatedAtUtc{ get; private set; }
        public DateTime? DeactivatedAtUtc{ get; private set; }

        public MerchantSector()
        {
            
        }

        public static MerchantSector Create(
            string code,
            string name,
            bool isHighRisk = false)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new BusinessException((int)MerchantErrorCode.SectorCodeRequired);
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BusinessException((int)MerchantErrorCode.SectorNameRequired);
            }
            return new MerchantSector
            {
                Id = Guid.NewGuid(),
                Code = code.Trim(),
                Name = name.Trim(),
                IsHighRisk = isHighRisk,
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
        public void EnsureAtive()
        {
            if (!IsActive)
            {
                throw new BusinessException((int)MerchantErrorCode.SectorNotActive);
            }
        }
    }
}
