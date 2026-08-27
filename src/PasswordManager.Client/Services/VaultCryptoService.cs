using System.Security.Cryptography;

namespace PasswordManager.Client.Services;

public class VaultCryptoService : IVaultCryptoService
{
    private const int NonceSize = 12; 
    private const int TagSize = 16;   

    public (string CipherTextBase64, string NonceBase64) WrapKey(byte[] keyToWrap, byte[] encryptionKey)
    {
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] cipherText = new byte[keyToWrap.Length];
        byte[] tag = new byte[TagSize];

        using var aesGcm = new AesGcm(encryptionKey, TagSize);
        aesGcm.Encrypt(nonce, keyToWrap, cipherText, tag);

        byte[] combined = new byte[cipherText.Length + tag.Length];
        Buffer.BlockCopy(cipherText, 0, combined, 0, cipherText.Length);
        Buffer.BlockCopy(tag, 0, combined, cipherText.Length, tag.Length);

        return (Convert.ToBase64String(combined), Convert.ToBase64String(nonce));
    }

    public byte[] UnwrapKey(string cipherTextBase64, string nonceBase64, byte[] encryptionKey)
    {
        byte[] combined = Convert.FromBase64String(cipherTextBase64);
        byte[] nonce = Convert.FromBase64String(nonceBase64);

        int cipherLength = combined.Length - TagSize;
        byte[] cipherText = new byte[cipherLength];
        byte[] tag = new byte[TagSize];
        Buffer.BlockCopy(combined, 0, cipherText, 0, cipherLength);
        Buffer.BlockCopy(combined, cipherLength, tag, 0, TagSize);

        byte[] plainText = new byte[cipherLength];

        using var aesGcm = new AesGcm(encryptionKey, TagSize);
        aesGcm.Decrypt(nonce, cipherText, tag, plainText);

        return plainText;
    }
}