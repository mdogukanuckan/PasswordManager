using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Persistence;

namespace PasswordManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}