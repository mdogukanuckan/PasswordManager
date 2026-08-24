using PasswordManager.Contracts.DTOs.VaultItem;

namespace PasswordManager.Application.Interfaces.Services;

public interface IVaultItemService
{
    Task<IEnumerable<VaultItemResponse>> GetAllAsync(Guid userId);
    Task<VaultItemResponse> GetByIdAsync(Guid id, Guid userId);
    Task<VaultItemResponse> CreateAsync(Guid userId, CreateVaultItemRequest request);
    Task<VaultItemResponse> UpdateAsync(Guid id,Guid userId, UpdateVaultItemRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}