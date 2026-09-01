using CommunityToolkit.Mvvm.Input;

namespace PasswordManager.Client.ViewModels;

public partial class ManageCategoriesViewModel
{
    public CategoryPickerViewModel CategoryPicker { get; }

    public ManageCategoriesViewModel(CategoryPickerViewModel categoryPicker)
    {
        CategoryPicker = categoryPicker;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}