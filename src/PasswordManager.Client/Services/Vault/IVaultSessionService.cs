namespace PasswordManager.Client.Services.Vault;

public interface IVaultSessionService
{
    byte[]? VaultKey { get; }
    void SetVaultKey(byte[] vaultKey);
    void Clear();
}