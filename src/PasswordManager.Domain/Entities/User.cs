namespace PasswordManager.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string AuthHash { get; set; } = string.Empty;
    public string AuthSalt { get; set; } = string.Empty;
    public string WrappedVaultKey { get; set; } = string.Empty;
    public string WrappedVaultKeyNonce { get; set; } = string.Empty;

    public string EncryptionSalt { get; set; } = string.Empty;

    public int KdfIterations { get; set; } = 2;
    public int KdfMemorySize { get; set; } = 19456;
    public int KdfParallelism { get; set; } = 1;

    public ICollection<VaultItem> VaultItems { get; set; } = new List<VaultItem>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}