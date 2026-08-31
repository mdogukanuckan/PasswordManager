using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Services.Auth;
using PasswordManager.Client.Services.Vault;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Contracts.DTOs.Auth;
using System.Security.Cryptography;

namespace PasswordManager.Client.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private const int KdfIterations = 2;
    private const int KdfMemorySizeKb = 19 * 1024;
    private const int KdfParallelism = 1;
    private const int SaltSize = 16;
    private const int VaultKeySize = 32;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    private readonly IAuthApiService _authApiService;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly IVaultCryptoService _vaultCryptoService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IVaultSessionService _vaultSessionService;

    public RegisterViewModel(
        IAuthApiService authApiService,
        IKeyDerivationService keyDerivationService,
        IVaultCryptoService vaultCryptoService,
        ITokenStorageService tokenStorageService,
        IVaultSessionService vaultSessionService)
    {
        _authApiService = authApiService;
        _keyDerivationService = keyDerivationService;
        _vaultCryptoService = vaultCryptoService;
        _tokenStorageService = tokenStorageService;
        _vaultSessionService = vaultSessionService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "E-posta ve ana şifre zorunludur.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Ana şifreler eşleşmiyor.";
            return;
        }

        IsBusy = true;

        try
        {
            byte[] authSalt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] encryptionSalt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] authKeyBytes = await _keyDerivationService.DeriveKeyAsync(
                Password, Convert.ToBase64String(authSalt), KdfIterations, KdfMemorySizeKb, KdfParallelism);

            byte[] encryptionKeyBytes = await _keyDerivationService.DeriveKeyAsync(
                Password, Convert.ToBase64String(encryptionSalt), KdfIterations, KdfMemorySizeKb, KdfParallelism);

            byte[] vaultKey = RandomNumberGenerator.GetBytes(VaultKeySize);
            var wrapped = _vaultCryptoService.WrapKey(vaultKey, encryptionKeyBytes);

            var request = new RegisterRequest(
                Email,
                Convert.ToBase64String(authKeyBytes),
                Convert.ToBase64String(authSalt),
                Convert.ToBase64String(encryptionSalt),
                wrapped.CipherTextBase64,
                wrapped.NonceBase64);

            var response = await _authApiService.RegisterAsync(request);
            _vaultSessionService.SetVaultKey(vaultKey);
            _vaultSessionService.SetUserEmail(Email);
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
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
