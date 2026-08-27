namespace PasswordManager.Client.Services;

public interface IVaultCryptoService
{
    (string CipherTextBase64, string NonceBase64) WrapKey(byte[] keyToWrap, byte[] encryptionKey);
    byte[] UnwrapKey(string cipherTextBase64, string nonceBase64, byte[] encryptionKey);
}