namespace PasswordManager.Client.Services.Auth;

public interface ITokenStorageService
{
    Task SaveTokensAsync(string accessToken, string refreshToken, bool rememberMe);
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();

    Task UpdateTokensAsync(string accessToken, string refreshToken);
    void ClearTokens();
    Task SaveEmailAsync(string email, bool rememberMe);
    Task<string?> GetRememberedEmailAsync();
}