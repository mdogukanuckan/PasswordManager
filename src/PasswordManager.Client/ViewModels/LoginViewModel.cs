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

    public LoginViewModel(IAuthApiService authApiService, ITokenStorageService tokenStorageService)
    {
        _authApiService = authApiService;
        _tokenStorageService = tokenStorageService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {

            // TODO: Argon2id key derivation ile AuthKey üretilecek, şimdilik geçici olarak Password gönderiliyor
            var response = await _authApiService.LoginAsync(new LoginRequest(Email, Password));
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
}