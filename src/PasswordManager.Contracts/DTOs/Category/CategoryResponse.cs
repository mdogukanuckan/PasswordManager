namespace PasswordManager.Contracts.DTOs.Category;

public record CategoryResponse(
    Guid Id,
    string EncryptedName,
    string Nonce,
    DateTime CreatedAt
);