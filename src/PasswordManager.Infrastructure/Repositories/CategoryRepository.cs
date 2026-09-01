using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Persistence;

namespace PasswordManager.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Category item)
    {
        await _context.Categories.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category item)
    {
        _context.Categories.Remove(item);
        
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Category>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Categories.Where(c => c.UserId == userId).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task UpdateAsync(Category item)
    {
        _context.Categories.Update(item);
        
        await _context.SaveChangesAsync();
    }
}