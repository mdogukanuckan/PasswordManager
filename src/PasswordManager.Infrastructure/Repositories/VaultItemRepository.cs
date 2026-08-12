using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Persistence;

namespace PasswordManager.Infrastructure.Repositories;

public class VaultItemRepository : IVaultItemRepository
{
    private readonly AppDbContext _context;
    public VaultItemRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(VaultItem item)
    {
        await _context.VaultItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(VaultItem item)
    {
        _context.VaultItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<VaultItem>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.VaultItems.Where(v => v.UserId == userId).ToListAsync();
    }

    public async Task<VaultItem?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.VaultItems.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
    }

    public async Task UpdateAsync(VaultItem item)
    {
        _context.VaultItems.Update(item);
        await _context.SaveChangesAsync();
    }
}