namespace PasswordManager.Client.Services.Auth;

public class TokenStorageService : ITokenStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private string? _sessionAccessToken;
    private string? _sessionRefreshToken;


    public void ClearTokens()
    {
        _sessionAccessToken = null;
        _sessionRefreshToken = null;
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_sessionAccessToken))
        {
            return _sessionAccessToken;
        }
        return await SecureStorage.Default.GetAsync(AccessTokenKey);

    }


    public async Task<string?> GetRefreshTokenAsync()
    {

        if (!string.IsNullOrEmpty(_sessionRefreshToken))
            return _sessionRefreshToken;
        return await SecureStorage.Default.GetAsync(RefreshTokenKey);
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, bool rememberMe)
    {
        if (rememberMe)
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
            await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
            _sessionAccessToken = null;
            _sessionRefreshToken = null;
        }
        else
        {
            _sessionAccessToken = accessToken;
            _sessionRefreshToken = refreshToken;
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
        }
    }
}