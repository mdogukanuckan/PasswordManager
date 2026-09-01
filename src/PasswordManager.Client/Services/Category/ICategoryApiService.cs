using PasswordManager.Contracts.DTOs.Category;

namespace PasswordManager.Client.Services.Category;

public interface ICategoryApiService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task DeleteAsync(Guid id);
}