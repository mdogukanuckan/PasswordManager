namespace PasswordManager.Client.Services.Vault;

public interface IVaultSessionService
{
    byte[]? VaultKey { get; }
    string? UserEmail { get; }
    void SetVaultKey(byte[] vaultKey);
    void SetUserEmail(string email);
    void Clear();
}
