using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.Modules.Providers.Contracts.Credentials
{
    public interface ICredentialProtector
    {
        string Protect(string credentialPayloadJson, string encryptionKeyVersion);
        string Unprotect(string encryptedCredentialPayload, string encryptionKeyVersion);
    }
}
