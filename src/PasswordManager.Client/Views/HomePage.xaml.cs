using PasswordManager.Client.ViewModels;

namespace PasswordManager.Client.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

}