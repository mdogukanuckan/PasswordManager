
using PasswordManager.Client.Views;

namespace PasswordManager.Client;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
		Routing.RegisterRoute(nameof(AddVaultItemPage), typeof(AddVaultItemPage));
	}
}
