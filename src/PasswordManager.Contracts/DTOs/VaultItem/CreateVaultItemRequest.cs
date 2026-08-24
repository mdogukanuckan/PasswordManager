namespace PasswordManager.Contracts.DTOs.VaultItem;

public record CreateVaultItemRequest(
    string EncryptedData,
    string Nonce);