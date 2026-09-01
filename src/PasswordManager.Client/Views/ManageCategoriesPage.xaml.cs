using PasswordManager.Client.ViewModels;

namespace PasswordManager.Client.Views;

public partial class ManageCategoriesPage : ContentPage
{
    private readonly ManageCategoriesViewModel _viewModel;

    public ManageCategoriesPage(ManageCategoriesViewModel viewModel)
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