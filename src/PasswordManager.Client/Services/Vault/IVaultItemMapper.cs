using PasswordManager.Client.Models;
using PasswordManager.Contracts.DTOs.VaultItem;

namespace PasswordManager.Client.Services.Vault;

public interface IVaultItemMapper
{
    CreateVaultItemRequest ToCreateRequest(VaultItemPayload payload, byte[] vaultKey);
    VaultItemPayload ToPayload(VaultItemResponse response, byte[] vaultKey);
    UpdateVaultItemRequest ToUpdateRequest(VaultItemPayload payload, byte[] vaultKey);
}