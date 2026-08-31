namespace PasswordManager.Client.Services.Vault;

public class VaultSessionService : IVaultSessionService
{
    public byte[]? VaultKey { get; private set; }
    public string? UserEmail { get; private set; }

    public void SetVaultKey(byte[] vaultKey)
    {
        VaultKey = vaultKey;
    }

    public void SetUserEmail(string email)
    {
        UserEmail = email;
    }

    public void Clear()
    {
        VaultKey = null;
        UserEmail = null;
    }
}
