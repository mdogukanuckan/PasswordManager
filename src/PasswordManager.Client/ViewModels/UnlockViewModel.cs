using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Services.Auth;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Client.Services.Vault;
using PasswordManager.Contracts.DTOs.Auth;

namespace PasswordManager.Client.ViewModels;

public partial class UnlockViewModel : ObservableObject, IQueryAttributable
{
    private readonly IAuthApiService _authApiService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly IVaultCryptoService _vaultCryptoService;
    private readonly IVaultSessionService _vaultSessionService;

    public UnlockViewModel(
        IAuthApiService authApiService,
        ITokenStorageService tokenStorageService,
        IKeyDerivationService keyDerivationService,
        IVaultCryptoService vaultCryptoService,
        IVaultSessionService vaultSessionService)
    {
        _authApiService = authApiService;
        _tokenStorageService = tokenStorageService;
        _keyDerivationService = keyDerivationService;
        _vaultCryptoService = vaultCryptoService;
        _vaultSessionService = vaultSessionService;
    }

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Email", out var email))
        {
            Email = (string)email;
        }
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            var salt = await _authApiService.GetSaltAsync(Email);

            var encryptionKey = await _keyDerivationService.DeriveKeyAsync(
                Password,
                salt.EncryptionSalt,
                salt.KdfIterations,
                salt.KdfMemorySize,
                salt.KdfParallelism);

            var response = await _authApiService.RefreshAsync(new RefreshRequest(refreshToken));

            await _tokenStorageService.SaveTokensAsync(response.AccessToken, response.RefreshToken, true);

            byte[] vaultKey;
            try
            {
                vaultKey = _vaultCryptoService.UnwrapKey(
                    response.WrappedVaultKey,
                    response.WrappedVaultKeyNonce,
                    encryptionKey);
            }
            catch (CryptographicException)
            {
                ErrorMessage = "Ana şifre yanlış.";
                return;
            }

            _vaultSessionService.SetVaultKey(vaultKey);
            _vaultSessionService.SetUserEmail(Email);

            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            _tokenStorageService.ClearTokens();
            await Shell.Current.GoToAsync("//LoginPage");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UseAnotherAccountAsync()
    {
        _tokenStorageService.ClearTokens();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}