namespace PasswordManager.Application.DTOs.VaultItem;

public record UpdateVaultItemRequest(
    string EncryptedData,
    string Nonce);