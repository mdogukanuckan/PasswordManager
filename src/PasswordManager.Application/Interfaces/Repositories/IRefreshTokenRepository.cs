using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAsync(RefreshToken refreshToken);
    Task RevokeAllByUserIdAsync(Guid userId);

}