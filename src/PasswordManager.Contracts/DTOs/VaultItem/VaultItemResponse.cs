namespace PasswordManager.Contracts.DTOs.VaultItem;

public record VaultItemResponse(
    Guid Id,
    string EncryptedData,
    string Nonce,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);