using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Models;
using PasswordManager.Client.Services.Vault;
using PasswordManager.Client.Services.Exceptions;

namespace PasswordManager.Client.ViewModels;

public partial class VaultItemDetailViewModel : ObservableObject, IQueryAttributable
{

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Username { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsPasswordMasked { get; set; } = true;
    [ObservableProperty]
    public partial Guid Id { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    private readonly IVaultItemMapper _vaultItemMapper;
    private readonly IVaultItemApiService _vaultItemApiService;
    private readonly IVaultSessionService _vaultSessionService;
    public CategoryPickerViewModel CategoryPicker { get; }

    private string _pendingCategory = CategoryPickerViewModel.DefaultCategory;

    public VaultItemDetailViewModel(
        IVaultItemMapper vaultItemMapper,
        IVaultItemApiService vaultItemApiService,
        IVaultSessionService vaultSessionService,
        CategoryPickerViewModel categoryPicker)
    {
        _vaultItemMapper = vaultItemMapper;
        _vaultItemApiService = vaultItemApiService;
        _vaultSessionService = vaultSessionService;
        CategoryPicker = categoryPicker;
    }


    [RelayCommand]
    private void ToggleMask()
    {
        IsPasswordMasked = !IsPasswordMasked;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue("Item", out var value);
        if (value is VaultItemListEntry entry)
        {
            Id = entry.Id;
            Title = entry.Payload.Title;
            Username = entry.Payload.Username;
            Password = entry.Payload.Password;
            Notes = entry.Payload.Notes;
            _pendingCategory = string.IsNullOrWhiteSpace(entry.Payload.Category)
                                ? CategoryPickerViewModel.DefaultCategory
                                : entry.Payload.Category;
        }
    }
    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Başlık boş bırakılamaz.";
                return;
            }

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
    Notes,
    string.IsNullOrWhiteSpace(CategoryPicker.SelectedCategory) ? CategoryPickerViewModel.DefaultCategory : CategoryPicker.SelectedCategory);

            var request = _vaultItemMapper.ToUpdateRequest(payload, vaultKey);

            await _vaultItemApiService.UpdateAsync(Id, request);

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

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
    [RelayCommand]
    private async Task DeleteAsync()
    {
        bool confirmed = await Shell.Current.CurrentPage.DisplayAlert(
            "Kaydı Sil",
            "Bu kaydı silmek istediğinize emin misiniz? Bu işlem geri alınamaz.",
            "Sil",
            "Vazgeç");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await _vaultItemApiService.DeleteAsync(Id);
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

    public async Task InitializeCategoryAsync()
    {
        await CategoryPicker.LoadCategoriesCommand.ExecuteAsync(null);
        CategoryPicker.SelectedCategory = _pendingCategory;
    }
}