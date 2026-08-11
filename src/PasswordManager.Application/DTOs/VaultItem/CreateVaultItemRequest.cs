namespace PasswordManager.Application.DTOs.VaultItem;

public record CreateVaultItemRequest(
    string EncryptedData,
    string Nonce);