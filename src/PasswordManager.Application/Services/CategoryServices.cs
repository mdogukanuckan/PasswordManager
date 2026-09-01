using PasswordManager.Application.Exceptions;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Application.Interfaces.Services;
using PasswordManager.Contracts.DTOs.Category;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<CategoryResponse> CreateAsync(Guid userId, CreateCategoryRequest request)
    {
        var category = new Category
        {
            UserId = userId,
            EncryptedName = request.EncryptedName,
            Nonce = request.Nonce
        };
        await _categoryRepository.AddAsync(category);
        return MapToResponse(category);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var category = await _categoryRepository.GetByIdAsync(id,userId);
        if(category is null)
        {
            throw new NotFoundException();
        }
        await _categoryRepository.DeleteAsync(category);
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync(Guid userId)
    {
        var categories = await  _categoryRepository.GetAllByUserIdAsync(userId);
        return categories.Select(category => MapToResponse(category));
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var category = await _categoryRepository.GetByIdAsync(id,userId);
        if(category is null){

            throw new NotFoundException();
        }
        return MapToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, Guid userId, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id,userId);
        if(category is null)
        {
            throw new NotFoundException();
        }
        category.EncryptedName = request.EncryptedName;
        category.Nonce = request.Nonce;
        category.ModifiedAt = DateTime.UtcNow;
        await _categoryRepository.UpdateAsync(category);

        return MapToResponse(category);
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.EncryptedName,
            category.Nonce,
            category.CreatedAt
        );
    }
}