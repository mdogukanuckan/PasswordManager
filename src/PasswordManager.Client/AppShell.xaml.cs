using PasswordManager.Client.Services.Auth;
using PasswordManager.Client.Views;

namespace PasswordManager.Client;

public partial class AppShell : Shell
{
    private readonly ITokenStorageService _tokenStorageService;

    public AppShell(ITokenStorageService tokenStorageService)
    {
        InitializeComponent();
        _tokenStorageService = tokenStorageService;

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(AddVaultItemPage), typeof(AddVaultItemPage));
        Routing.RegisterRoute(nameof(VaultItemDetailPage), typeof(VaultItemDetailPage));
        Routing.RegisterRoute(nameof(ManageCategoriesPage), typeof(ManageCategoriesPage));

        this.Loaded += async (s, e) => await CheckAutoUnlockAsync();
    }

    private async Task CheckAutoUnlockAsync()
    {
        var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
        var email = await _tokenStorageService.GetRememberedEmailAsync();

        if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(email))
        {
            await Shell.Current.GoToAsync($"//UnlockPage?Email={Uri.EscapeDataString(email)}");
        }
        // else: hiçbir şey yapma, kullanıcı zaten Shell açılışta LoginPage'i görecek
        // (varsayım: AppShell.xaml'de ilk ShellContent LoginPage)
    }
}
