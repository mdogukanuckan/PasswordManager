using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Interfaces.Repositories;
using PasswordManager.Domain.Entities;
using PasswordManager.Infrastructure.Persistence;

namespace PasswordManager.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    }

    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        await _context.RefreshTokens
                .Where(r => r.UserId == userId && r.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.RevokedAt, DateTime.UtcNow));
    }

    public async Task RevokeAsync(RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();

    }
}