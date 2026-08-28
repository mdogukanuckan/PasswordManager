using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Models;
using PasswordManager.Client.Services;

namespace PasswordManager.Client.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IVaultItemApiService _vaultItemApiService;
    private readonly IVaultItemMapper _vaultItemMapper;
    private readonly IVaultSessionService _vaultSessionService;

    [ObservableProperty]
    public partial ObservableCollection<VaultItemListEntry> VaultItems { get; set; } = new();

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public HomeViewModel(
        IVaultItemApiService vaultItemApiService,
        IVaultItemMapper vaultItemMapper,
        IVaultSessionService vaultSessionService)
    {
        _vaultItemApiService = vaultItemApiService;
        _vaultItemMapper = vaultItemMapper;
        _vaultSessionService = vaultSessionService;
    }

    [RelayCommand]
    private async Task LoadVaultItemsAsync()
    {
        if (_vaultSessionService.VaultKey is null)
        {
            ErrorMessage = "Vault key bulunamadı, lütfen tekrar giriş yapın.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var responses = await _vaultItemApiService.GetAllAsync();
            VaultItems.Clear();

            foreach (var response in responses)
            {
                var payload = _vaultItemMapper.ToPayload(response, _vaultSessionService.VaultKey);
                VaultItems.Add(new VaultItemListEntry(response.Id, payload));
            }
        }
        catch (Services.Exceptions.ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}