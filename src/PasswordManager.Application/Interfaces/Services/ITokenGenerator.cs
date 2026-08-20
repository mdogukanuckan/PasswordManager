using PasswordManager.Domain.Entities;

namespace PasswordManager.Application.Interfaces.Services;

public interface ITokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}