using PasswordManager.Contracts.DTOs.Auth;

namespace PasswordManager.Application.Interfaces.Services;

public interface IAuthService
{
    Task<SaltResponse> GetSaltAsync(string email);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
}