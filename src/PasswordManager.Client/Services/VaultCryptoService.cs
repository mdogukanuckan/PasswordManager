using System.Security.Cryptography;

namespace PasswordManager.Client.Services;

public class VaultCryptoService : IVaultCryptoService
{
    private const int NonceSize = 12; // AES-GCM standardı, 96 bit
    private const int TagSize = 16;   // 128 bit

    public (string CipherTextBase64, string NonceBase64) WrapKey(byte[] keyToWrap, byte[] encryptionKey)
        => Encrypt(keyToWrap, encryptionKey);

    public byte[] UnwrapKey(string cipherTextBase64, string nonceBase64, byte[] encryptionKey)
        => Decrypt(cipherTextBase64, nonceBase64, encryptionKey);

    public (string CipherTextBase64, string NonceBase64) Encrypt(byte[] plaintext, byte[] key)
    {
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] cipherText = new byte[plaintext.Length];
        byte[] tag = new byte[TagSize];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Encrypt(nonce, plaintext, cipherText, tag);

        byte[] combined = new byte[cipherText.Length + tag.Length];
        Buffer.BlockCopy(cipherText, 0, combined, 0, cipherText.Length);
        Buffer.BlockCopy(tag, 0, combined, cipherText.Length, tag.Length);

        return (Convert.ToBase64String(combined), Convert.ToBase64String(nonce));
    }

    public byte[] Decrypt(string cipherTextBase64, string nonceBase64, byte[] key)
    {
        byte[] combined = Convert.FromBase64String(cipherTextBase64);
        byte[] nonce = Convert.FromBase64String(nonceBase64);

        int cipherLength = combined.Length - TagSize;
        byte[] cipherText = new byte[cipherLength];
        byte[] tag = new byte[TagSize];
        Buffer.BlockCopy(combined, 0, cipherText, 0, cipherLength);
        Buffer.BlockCopy(combined, cipherLength, tag, 0, TagSize);

        byte[] plainText = new byte[cipherLength];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Decrypt(nonce, cipherText, tag, plainText);

        return plainText;
    }
}