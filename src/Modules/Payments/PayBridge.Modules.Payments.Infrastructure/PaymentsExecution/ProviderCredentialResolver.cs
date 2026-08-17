using PayBridge.BuildingBlocks.Exceptions;
using PayBridge.Modules.Merchants.Contracts.Credentials;
using PayBridge.Modules.Merchants.Contracts.Merchants.Abstraction;
using PayBridge.Modules.Payments.Application.Abstractions;
using PayBridge.Modules.Providers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Payments.Infrastructure.PaymentsExecution
{
    internal sealed class ProviderCredentialResolver : IProviderCredentialResolver
    {
        private const int MerchantIdRequired = 200000;
        private const int ProviderCodeRequired = 200050;
        private const int ProviderAccountNotFound = 200051;
        private const int ProviderCredentialNotActive = 200062;

        private readonly IMerchantProviderAccountReader _merchantProviderAccountReader;
        private ICredentialProtector _credentialProtector;
        public ProviderCredentialResolver(IMerchantProviderAccountReader merchantProviderAccountReader,
            ICredentialProtector credentialProtector)
        {
            _merchantProviderAccountReader = merchantProviderAccountReader;
            _credentialProtector = credentialProtector;
        }
        public async Task<ProviderCredentialContext> ResolveAsync(Guid merchantId, string providerCode, string channel, CancellationToken cancellationToken)
        {
            if (merchantId ==Guid.Empty)
            {
                throw new BusinessException(MerchantIdRequired);
            }
            if (string.IsNullOrWhiteSpace(providerCode))
            {
                throw new BusinessException(ProviderCodeRequired);
            }
             var providerAccount = 
                await _merchantProviderAccountReader.GetActiveProviderAccountAsync(
                    merchantId,
                    providerCode, 
                    channel,
                    cancellationToken);

            if (providerAccount == null)
            {
                throw new BusinessException(ProviderAccountNotFound);
            }
            var credential = await _merchantProviderAccountReader.GetActiveCredentialAsync(
                providerAccount.Id,
                cancellationToken);
            if (credential == null || !credential.IsActive) 
            {
                throw new BusinessException(ProviderCredentialNotActive);
            }
            var credentialPayloadJson = _credentialProtector.Unprotect(
                credential.EncryptedCredentialPayload,
                credential.EncryptionKeyVersion);

            return new ProviderCredentialContext(
                providerAccount.Id,
                providerAccount.ProviderCode,
                credentialPayloadJson);
        }
    }
}
