namespace PasswordManager.Contracts.DTOs.Category;

public record CreateCategoryRequest(
    string EncryptedName,
    string Nonce
);