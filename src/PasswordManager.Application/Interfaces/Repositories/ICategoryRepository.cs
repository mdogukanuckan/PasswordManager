using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Category>> GetAllByUserIdAsync(Guid userId);
    Task AddAsync(Category item);
    Task UpdateAsync(Category item);
    Task DeleteAsync(Category item);
    Task DeleteAllByUserIdAsync(Guid userId);
}