using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Merchants.Domain.Merchants.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Merchants.Domain.Merchants.Entities
{
    public sealed class MerchantProviderCredential
    {
        public Guid Id { get;private set; }

        public Guid MerchantProviderAccountId { get;private set; }

        public string EncryptedCredentialPayload { get;private set; } = default!;
        public string EncryptedKeyVersion { get; private set; } = default!;
        public bool IsActive { get;private set; }

        public DateTime CreatedAtUtc { get;private set; }
        public DateTime RotatedAtUtc { get;private set; }
        public DateTime RevokedAtUtc { get;private set; }

        public MerchantProviderCredential()
        {
        }

        public static MerchantProviderCredential Create(
            Guid merchantProviderAccountId,
            string encryptedCredentialPayload,
            string encryptedKeyVersion)
        {
            if (merchantProviderAccountId == Guid.Empty)
            {
                throw new BusinessException((int)MerchantErrorCode.ProviderAccountNotFound);
            }
            if (string.IsNullOrWhiteSpace(encryptedCredentialPayload))
            {
                throw new BusinessException((int)MerchantErrorCode.EncryptedCredentialPayloadRequired);
            }
            if (string.IsNullOrWhiteSpace(encryptedKeyVersion))
            {
                throw new BusinessException((int)MerchantErrorCode.EncryptionKeyVersionRequired);
            }

            return new MerchantProviderCredential
            {
                Id = Guid.NewGuid(),
                MerchantProviderAccountId = merchantProviderAccountId,
                EncryptedCredentialPayload = encryptedCredentialPayload.Trim(),
                EncryptedKeyVersion = encryptedKeyVersion.Trim(),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        public void Rotate(
            string encryptedCredentialPayload,
            string encryptedKeyVersion)
        {
            if (string.IsNullOrWhiteSpace(encryptedCredentialPayload))
            {
                throw new BusinessException((int)MerchantErrorCode.EncryptedCredentialPayloadRequired);
            }

            if (string.IsNullOrWhiteSpace(encryptedKeyVersion))
            {
                throw new BusinessException((int)MerchantErrorCode.EncryptionKeyVersionRequired);
            }

            EncryptedCredentialPayload = encryptedCredentialPayload;
            EncryptedKeyVersion = encryptedKeyVersion;
            RotatedAtUtc = DateTime.UtcNow;
        }
        public void Revoke()
        {
            if (!IsActive)
            {
                return;
            }
            IsActive = false;
            RevokedAtUtc = DateTime.UtcNow;
        }
        public void EnsureActive()
        {
            if (!IsActive)
            {
                throw new BusinessException((int)MerchantErrorCode.ProviderCredentialNotActive);
            }
        }
    }
}
