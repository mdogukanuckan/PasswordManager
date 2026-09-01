namespace PasswordManager.Domain.Entities;

public class Category : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string EncryptedName { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;

}