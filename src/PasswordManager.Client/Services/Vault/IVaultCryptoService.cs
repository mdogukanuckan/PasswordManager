namespace PasswordManager.Client.Services.Vault;

public interface IVaultCryptoService
{
    (string CipherTextBase64, string NonceBase64) WrapKey(byte[] keyToWrap, byte[] encryptionKey);
    byte[] UnwrapKey(string cipherTextBase64, string nonceBase64, byte[] encryptionKey);
     (string CipherTextBase64, string NonceBase64) Encrypt(byte[] plaintext, byte[] key);
    byte[] Decrypt(string cipherTextBase64, string nonceBase64, byte[] key);
}