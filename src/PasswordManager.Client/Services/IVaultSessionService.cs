namespace PasswordManager.Client.Services;

public interface IVaultSessionService
{
    byte[]? VaultKey { get; }
    void SetVaultKey(byte[] vaultKey);
    void Clear();
}