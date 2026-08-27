using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Services;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Contracts.DTOs.Auth;

namespace PasswordManager.Client.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    private readonly IAuthApiService _authApiService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly IVaultCryptoService _vaultCryptoService;

    private byte[]? _encryptionKey;
    private byte[]? _vaultKey;

    public LoginViewModel(
         IAuthApiService authApiService,
         ITokenStorageService tokenStorageService,
         IKeyDerivationService keyDerivationService,
         IVaultCryptoService vaultCryptoService)
    {
        _authApiService = authApiService;
        _tokenStorageService = tokenStorageService;
        _keyDerivationService = keyDerivationService;
        _vaultCryptoService = vaultCryptoService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var salt = await _authApiService.GetSaltAsync(Email);
            var authKeyBytes = await _keyDerivationService.DeriveKeyAsync(
                Password, salt.AuthSalt,salt.KdfIterations,salt.KdfMemorySize,salt.KdfParallelism);

            _encryptionKey = await _keyDerivationService.DeriveKeyAsync(
                Password, salt.EncryptionSalt,salt.KdfIterations,salt.KdfMemorySize,salt.KdfParallelism);
            var authKey = Convert.ToBase64String(authKeyBytes);
            var response = await _authApiService.LoginAsync(new LoginRequest(Email, authKey));
            _vaultKey = _vaultCryptoService.UnwrapKey(response.WrappedVaultKey,response.WrappedVaultKeyNonce,_encryptionKey);
            await _tokenStorageService.SaveTokensAsync(response.AccessToken, response.RefreshToken);
            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
private async Task GoToRegisterAsync()
{
    await Shell.Current.GoToAsync("RegisterPage");
}
}