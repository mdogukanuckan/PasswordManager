using System.Text;
using System.Text.Json;
using PasswordManager.Client.Models;
using PasswordManager.Contracts.DTOs.VaultItem;

namespace PasswordManager.Client.Services.Vault;

public class VaultItemMapper : IVaultItemMapper
{
    private readonly IVaultCryptoService _vaultCryptoService;

    public VaultItemMapper(IVaultCryptoService vaultCryptoService)
    {
        _vaultCryptoService = vaultCryptoService;
    }

    public CreateVaultItemRequest ToCreateRequest(VaultItemPayload payload, byte[] vaultKey)
    {
        byte[] plaintextBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var encrypted = _vaultCryptoService.Encrypt(plaintextBytes, vaultKey);

        return new CreateVaultItemRequest(encrypted.CipherTextBase64, encrypted.NonceBase64);
    }

    public VaultItemPayload ToPayload(VaultItemResponse response, byte[] vaultKey)
    {
        byte[] plaintextBytes = _vaultCryptoService.Decrypt(response.EncryptedData, response.Nonce, vaultKey);
        return JsonSerializer.Deserialize<VaultItemPayload>(plaintextBytes)!;
    }

    public UpdateVaultItemRequest ToUpdateRequest(VaultItemPayload payload, byte[] vaultKey)
    {
        byte[] plaintextBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var encrypted = _vaultCryptoService.Encrypt(plaintextBytes, vaultKey);

        return new UpdateVaultItemRequest(encrypted.CipherTextBase64, encrypted.NonceBase64);
    }
}