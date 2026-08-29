using PasswordManager.Client.ViewModels;

namespace PasswordManager.Client.Views;

public partial class AddVaultItemPage : ContentPage
{
    public AddVaultItemPage(AddVaultItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}