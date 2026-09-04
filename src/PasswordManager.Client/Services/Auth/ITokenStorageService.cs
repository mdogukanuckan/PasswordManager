namespace PasswordManager.Client.Services.Auth;

public interface ITokenStorageService
{
    Task SaveTokensAsync(string accessToken, string refreshToken,bool rememberMe);
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    void ClearTokens();
}