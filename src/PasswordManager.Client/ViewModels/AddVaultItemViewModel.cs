using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Models;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Client.Services.PasswordGeneration;
using PasswordManager.Client.Services.Vault;


namespace PasswordManager.Client.ViewModels;

public partial class AddVaultItemViewModel : ObservableObject
{

    private const int GeneratedPasswordLength = 16;
    private const string PasswordCharset = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*-_=+";

    private readonly IVaultItemMapper _vaultItemMapper;
    private readonly IVaultItemApiService _vaultItemApiService;
    private readonly IVaultSessionService _vaultSessionService;
    private readonly IPasswordGeneratorService _passwordGeneratorService;


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
    public CategoryPickerViewModel CategoryPicker { get; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public AddVaultItemViewModel(
    IVaultItemMapper vaultItemMapper,
    IVaultItemApiService vaultItemApiService,
    IVaultSessionService vaultSessionService,
    CategoryPickerViewModel categoryPicker,
    IPasswordGeneratorService passwordGeneratorService
)
    {
        _vaultItemMapper = vaultItemMapper;
        _vaultItemApiService = vaultItemApiService;
        _vaultSessionService = vaultSessionService;
        _passwordGeneratorService = passwordGeneratorService;
        CategoryPicker = categoryPicker;
        

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

    [RelayCommand]
    private void GeneratePassword()
    {
        Password = _passwordGeneratorService.Generate();
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    
}
