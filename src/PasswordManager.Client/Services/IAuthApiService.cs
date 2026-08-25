using PasswordManager.Contracts.DTOs.Auth;

namespace PasswordManager.Client.Services;

public interface IAuthApiService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshAsync(RefreshRequest request);
    Task<SaltResponse> GetSaltAsync(string email);
    
}