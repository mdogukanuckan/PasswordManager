namespace PasswordManager.Client.Services.Vault;

public class VaultSessionService : IVaultSessionService
{
    public byte[]? VaultKey { get; private set; }

    public void SetVaultKey(byte[] vaultKey)
    {
        VaultKey = vaultKey;
    }

    public void Clear()
    {
        VaultKey = null;
    }
}