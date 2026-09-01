using PasswordManager.Client.ViewModels;

namespace PasswordManager.Client.Views;

public partial class VaultItemDetailPage : ContentPage
{
    private readonly VaultItemDetailViewModel _viewModel;

    public VaultItemDetailPage(VaultItemDetailViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCategoryAsync();
    }
}