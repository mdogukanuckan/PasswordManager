using PasswordManager.Contracts.DTOs.VaultItem;

namespace PasswordManager.Client.Services.Vault;

public interface IVaultItemApiService
{
    Task<IReadOnlyList<VaultItemResponse>> GetAllAsync();
    Task<VaultItemResponse> GetByIdAsync(Guid id);
    Task<VaultItemResponse> CreateAsync(CreateVaultItemRequest request);
    Task<VaultItemResponse> UpdateAsync(Guid id, UpdateVaultItemRequest request);
    Task DeleteAsync(Guid id);
}