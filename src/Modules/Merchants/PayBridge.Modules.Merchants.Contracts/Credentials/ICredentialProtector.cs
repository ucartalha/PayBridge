namespace PayBridge.Modules.Merchants.Contracts.Credentials;

public interface ICredentialProtector
{
    string Protect(
        string credentialPayloadJson,
        string encryptionKeyVersion);

    string Unprotect(
        string encryptedCredentialPayload,
        string encryptionKeyVersion);
}