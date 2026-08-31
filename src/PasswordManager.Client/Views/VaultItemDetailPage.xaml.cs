using PasswordManager.Client.ViewModels;

namespace PasswordManager.Client.Views;

public partial class VaultItemDetailPage : ContentPage
{
    public VaultItemDetailPage(VaultItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}