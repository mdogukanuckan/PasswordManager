namespace PasswordManager.Contracts.DTOs.VaultItem;

public record UpdateVaultItemRequest(
    string EncryptedData,
    string Nonce);