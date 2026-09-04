using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Interfaces.Repositories;

public interface IVaultItemRepository
{
    Task<VaultItem?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<VaultItem>> GetAllByUserIdAsync(Guid userId);
    Task AddAsync(VaultItem item);
    Task UpdateAsync(VaultItem item);
    Task DeleteAsync(VaultItem item);
    Task DeleteAllByUserIdAsync(Guid userId);
}