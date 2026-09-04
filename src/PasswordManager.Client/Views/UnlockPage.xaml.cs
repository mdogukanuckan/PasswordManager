using PasswordManager.Client.ViewModels;

namespace PasswordManager.Client.Views;

public partial class UnlockPage : ContentPage
{
    public UnlockPage(UnlockViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}