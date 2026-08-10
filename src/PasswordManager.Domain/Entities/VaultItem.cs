namespace PasswordManager.Domain.Entities;

public class VaultItem : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string EncryptedData { get; set; } = string.Empty;

    public string Nonce { get; set; } = string.Empty;
}