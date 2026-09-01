using PasswordManager.Contracts.DTOs.Category;

namespace PasswordManager.Client.Services.Category;

public interface ICategoryMapper
{
    CreateCategoryRequest ToCreateRequest(string name, byte[] vaultKey);
    string ToPlainTextName(CategoryResponse response, byte[] vaultKey);
}