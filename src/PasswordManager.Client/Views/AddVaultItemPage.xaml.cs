using PasswordManager.Client.ViewModels;

namespace PasswordManager.Client.Views;

public partial class AddVaultItemPage : ContentPage
{
    private readonly AddVaultItemViewModel _viewModel;
    public AddVaultItemPage(AddVaultItemViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
{
    base.OnAppearing();
    await _viewModel.CategoryPicker.LoadCategoriesCommand.ExecuteAsync(null);
}
}