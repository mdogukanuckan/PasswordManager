using PasswordManager.Contracts.DTOs.Category;

namespace PasswordManager.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync(Guid userId);
    Task<CategoryResponse> GetByIdAsync(Guid id, Guid userId);
    Task<CategoryResponse> CreateAsync(Guid userId, CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(Guid id, Guid userId, UpdateCategoryRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}