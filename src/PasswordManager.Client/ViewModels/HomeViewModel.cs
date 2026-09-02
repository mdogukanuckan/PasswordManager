using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Models;
using PasswordManager.Client.Services.Vault;
using PasswordManager.Client.Views;

namespace PasswordManager.Client.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IVaultItemApiService _vaultItemApiService;
    private readonly IVaultItemMapper _vaultItemMapper;
    private readonly IVaultSessionService _vaultSessionService;

    [ObservableProperty]
    public partial ObservableCollection<VaultItemListEntry> VaultItems { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<VaultItemListEntry> FilteredItems { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<VaultCategory> Categories { get; set; } = new();

    [ObservableProperty]
    public partial VaultCategory? SelectedCategory { get; set; }

    [ObservableProperty]
    public partial VaultItemListEntry? SelectedEntry { get; set; }

    [ObservableProperty]
    public partial bool HasSelection { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial string? UserName { get; set; }

    [ObservableProperty]
    public partial string UserInitial { get; set; } = "?";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }
    public CategoryPickerViewModel CategoryPicker { get; }

    public HomeViewModel(
    IVaultItemApiService vaultItemApiService,
    IVaultItemMapper vaultItemMapper,
    IVaultSessionService vaultSessionService,
    CategoryPickerViewModel categoryPicker)
    {
        _vaultItemApiService = vaultItemApiService;
        _vaultItemMapper = vaultItemMapper;
        _vaultSessionService = vaultSessionService;
        CategoryPicker = categoryPicker;
    }

    [RelayCommand]
private async Task LoadVaultItemsAsync()
{
    if (_vaultSessionService.VaultKey is null)
    {
        ErrorMessage = "Vault key bulunamadı, lütfen tekrar giriş yapın.";
        return;
    }

    IsBusy = true;
    ErrorMessage = null;
    UserName = _vaultSessionService.UserEmail;

    try
    {
        var vaultItemsTask = _vaultItemApiService.GetAllAsync();
        var categoriesTask = CategoryPicker.LoadCategoriesCommand.ExecuteAsync(null);

        await Task.WhenAll(vaultItemsTask, categoriesTask);

        var responses = await vaultItemsTask;

        var items = new List<VaultItemListEntry>();

        foreach (var response in responses)
        {
            var payload = _vaultItemMapper.ToPayload(
                response,
                _vaultSessionService.VaultKey);

            items.Add(
                new VaultItemListEntry(
                    response.Id,
                    payload,
                    response.CreatedAt,
                    response.ModifiedAt));
        }

        VaultItems = new ObservableCollection<VaultItemListEntry>(items);

        RecomputeCategories();
        SelectedCategory = Categories.FirstOrDefault();
        RefreshFilteredItems();
    }
    catch (Services.Exceptions.ApiException ex)
    {
        ErrorMessage = ex.Message;
    }
    finally
    {
        IsBusy = false;
    }
}

    [RelayCommand]
    private async Task GoToAddItemAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddVaultItemPage));
    }

    [RelayCommand]
    private async Task OpenDetailAsync(VaultItemListEntry? entry)
    {
        entry ??= SelectedEntry;
        if (entry is null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(VaultItemDetailPage), new Dictionary<string, object> { { "Item", entry } });
    }

    [RelayCommand]
    private async Task LockAsync()
    {
        _vaultSessionService.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    partial void OnSelectedEntryChanged(VaultItemListEntry? value)
    {
        HasSelection = value is not null;
    }

    partial void OnSearchTextChanged(string? value)
    {
        RefreshFilteredItems();
    }

    partial void OnSelectedCategoryChanged(VaultCategory? value)
    {
        RefreshFilteredItems();
    }

    partial void OnUserNameChanged(string? value)
    {
        UserInitial = string.IsNullOrWhiteSpace(value) ? "?" : value.Trim()[0].ToString().ToUpperInvariant();
    }

    private void RecomputeCategories()
{
    var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    foreach (var name in CategoryPicker.AvailableCategories)
    {
        counts[name] = 0;
    }

    foreach (var item in VaultItems)
    {
        var name = string.IsNullOrWhiteSpace(item.Payload.Category) ? "Diğer" : item.Payload.Category;
        counts[name] = counts.TryGetValue(name, out var existing) ? existing + 1 : 1;
    }

    var categories = new List<VaultCategory> { new("Tümü", VaultItems.Count) };
    categories.AddRange(counts
        .OrderBy(kv => kv.Key)
        .Select(kv => new VaultCategory(kv.Key, kv.Value)));

    Categories = new ObservableCollection<VaultCategory>(categories);
}

    private void RefreshFilteredItems()
    {
        IEnumerable<VaultItemListEntry> query = VaultItems;

        if (SelectedCategory is not null && SelectedCategory.Name != "Tümü")
        {
            query = query.Where(i => string.Equals(i.Payload.Category, SelectedCategory.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(i =>
                i.Payload.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                i.Payload.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        FilteredItems = new ObservableCollection<VaultItemListEntry>(query);
    }
    [RelayCommand]
    private async Task GoToManageCategoriesAsync()
    {
        await Shell.Current.GoToAsync(nameof(ManageCategoriesPage));
    }
}
