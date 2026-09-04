using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Persistence;

namespace PasswordManager.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.PasswordResetTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    }

    public async Task MarkAsUsedAsync(PasswordResetToken token)
    {
        token.UsedAt = DateTime.UtcNow;
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }
}