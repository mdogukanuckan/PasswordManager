using System.Text;
using PasswordManager.Client.Services.Vault;
using PasswordManager.Contracts.DTOs.Category;

namespace PasswordManager.Client.Services.Category;

public class CategoryMapper : ICategoryMapper
{
    private readonly IVaultCryptoService _vaultCryptoService;

    public CategoryMapper(IVaultCryptoService vaultCryptoService)
    {
        _vaultCryptoService = vaultCryptoService;
    }

    public CreateCategoryRequest ToCreateRequest(string name, byte[] vaultKey)
    {
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(name);
        var encrypted = _vaultCryptoService.Encrypt(plaintextBytes, vaultKey);
        return new CreateCategoryRequest(encrypted.CipherTextBase64, encrypted.NonceBase64);
    }

    public string ToPlainTextName(CategoryResponse response, byte[] vaultKey)
    {
        byte[] plaintextBytes = _vaultCryptoService.Decrypt(response.EncryptedName, response.Nonce, vaultKey);
        return Encoding.UTF8.GetString(plaintextBytes);
    }
}