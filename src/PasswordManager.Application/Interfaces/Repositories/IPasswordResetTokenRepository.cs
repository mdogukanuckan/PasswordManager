using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(PasswordResetToken token);
    Task MarkAsUsedAsync(PasswordResetToken token);
}