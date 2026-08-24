using PasswordManager.Application.DTOs.VaultItem;
using PasswordManager.Application.Exceptions;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Application.Interfaces.Services;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Services;

public class VaultItemService : IVaultItemService
{
    private readonly IVaultItemRepository _vaultItemRepository;
    public VaultItemService(IVaultItemRepository vaultItemRepository)
    {
        _vaultItemRepository = vaultItemRepository;
    }
    public async Task<VaultItemResponse> CreateAsync(Guid userId, CreateVaultItemRequest request)
    {
        var item = new VaultItem{
             UserId = userId,
             EncryptedData = request.EncryptedData,
             Nonce = request.Nonce
        };
        await _vaultItemRepository.AddAsync(item);
        return MapToResponse(item);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var item = await _vaultItemRepository.GetByIdAsync(id,userId);
        if(item is null)
        {
            throw new NotFoundException();
        }
        await _vaultItemRepository.DeleteAsync(item);
    }

    public async Task<IEnumerable<VaultItemResponse>> GetAllAsync(Guid userId)
    {
        var items = await _vaultItemRepository.GetAllByUserIdAsync(userId);
        return items.Select(item => MapToResponse(item));
    }

    public async Task<VaultItemResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var item = await _vaultItemRepository.GetByIdAsync(id,userId);
        if(item is null)
        {
            throw new NotFoundException();
        }
        return MapToResponse(item);
    }

    public async Task<VaultItemResponse> UpdateAsync(Guid id, Guid userId, UpdateVaultItemRequest request)
    {
        var item = await _vaultItemRepository.GetByIdAsync(id,userId);
        if(item is null)
        {
            throw new NotFoundException();
        }
        item.EncryptedData = request.EncryptedData;
        item.Nonce = request.Nonce;
        item.ModifiedAt = DateTime.UtcNow;
        await _vaultItemRepository.UpdateAsync(item);
        return MapToResponse(item);
    }

    private static VaultItemResponse MapToResponse(VaultItem item)
    {
        return new VaultItemResponse(
            item.Id,
            item.EncryptedData,
            item.Nonce,
            item.CreatedAt,
            item.ModifiedAt
        );
    }
}