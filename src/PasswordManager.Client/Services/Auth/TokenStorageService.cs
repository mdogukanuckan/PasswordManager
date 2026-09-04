namespace PasswordManager.Client.Services.Auth;

public class TokenStorageService : ITokenStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private string? _sessionAccessToken;
    private string? _sessionRefreshToken;
    private bool _rememberMe;


    public void ClearTokens()
    {
        _sessionAccessToken = null;
        _sessionRefreshToken = null;
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        Preferences.Default.Remove("remembered_email");

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

    public Task<string?> GetRememberedEmailAsync()
    {
        var email = Preferences.Default.Get<string?>("remembered_email", null);
        return Task.FromResult(email);
    }
    public Task SaveEmailAsync(string email, bool rememberMe)
    {
        if (rememberMe)
        {
            Preferences.Default.Set("remembered_email", email);
            return Task.CompletedTask;
        }
        Preferences.Default.Remove("remembered_email");
        return Task.CompletedTask;
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, bool rememberMe)
    {
        _rememberMe = rememberMe;
        if (_rememberMe)
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

    public Task UpdateTokensAsync(string accessToken, string refreshToken)
    {
        return SaveTokensAsync(accessToken, refreshToken, _rememberMe);
    }
}