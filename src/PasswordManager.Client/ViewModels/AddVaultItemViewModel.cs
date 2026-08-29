using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Models;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Client.Services.Vault;

namespace PasswordManager.Client.ViewModels;

public partial class AddVaultItemViewModel : ObservableObject
{
    private readonly IVaultItemMapper _vaultItemMapper;
    private readonly IVaultItemApiService _vaultItemApiService;
    private readonly IVaultSessionService _vaultSessionService;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Username { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public AddVaultItemViewModel(
        IVaultItemMapper vaultItemMapper,
        IVaultItemApiService vaultItemApiService,
        IVaultSessionService vaultSessionService
    )
    {

        _vaultItemMapper = vaultItemMapper;
        _vaultItemApiService = vaultItemApiService;
        _vaultSessionService = vaultSessionService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var vaultKey = _vaultSessionService.VaultKey;
            if (vaultKey is null)
            {
                ErrorMessage = "Vault anahtarı bulunamadı.";
                return;
            }
            var payload = new VaultItemPayload(
                Title,
                Username,
                Password,
                Notes);

            var request = _vaultItemMapper.ToCreateRequest(
                payload,
                vaultKey);

            await _vaultItemApiService.CreateAsync(request);

            await Shell.Current.GoToAsync("..");
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