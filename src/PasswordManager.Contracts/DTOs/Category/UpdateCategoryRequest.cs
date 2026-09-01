namespace PasswordManager.Contracts.DTOs.Category;
public record UpdateCategoryRequest(
    string EncryptedName,
    string Nonce
);