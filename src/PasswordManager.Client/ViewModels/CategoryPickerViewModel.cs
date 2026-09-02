using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Services.Category;
using PasswordManager.Client.Services.Exceptions;
using PasswordManager.Client.Services.Vault;

namespace PasswordManager.Client.ViewModels;

public partial class CategoryPickerViewModel : ObservableObject
{
    public const string DefaultCategory = "Kişisel";

    private readonly ICategoryApiService _categoryApiService;
    private readonly ICategoryMapper _categoryMapper;
    private readonly IVaultSessionService _vaultSessionService;

    private Dictionary<string, Guid> _categoryIdsByName = new();

    [ObservableProperty]
    public partial ObservableCollection<string> AvailableCategories { get; set; } = new();

    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = DefaultCategory;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public CategoryPickerViewModel(
        ICategoryApiService categoryApiService,
        ICategoryMapper categoryMapper,
        IVaultSessionService vaultSessionService)
    {
        _categoryApiService = categoryApiService;
        _categoryMapper = categoryMapper;
        _vaultSessionService = vaultSessionService;
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        var vaultKey = _vaultSessionService.VaultKey;
        if (vaultKey is null) return;

        var responses = await _categoryApiService.GetAllAsync();
        _categoryIdsByName = responses.ToDictionary(
            r => _categoryMapper.ToPlainTextName(r, vaultKey),
            r => r.Id);

        AvailableCategories = new ObservableCollection<string>(_categoryIdsByName.Keys);
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        string? name = await Shell.Current.CurrentPage.DisplayPromptAsync(
            "Yeni Kategori", "Kategori adını girin:", "Ekle", "Vazgeç");

        if (string.IsNullOrWhiteSpace(name)) return;

        var vaultKey = _vaultSessionService.VaultKey;
        if (vaultKey is null)
        {
            ErrorMessage = "Vault anahtarı bulunamadı.";
            return;
        }

        try
        {
            var request = _categoryMapper.ToCreateRequest(name, vaultKey);
            var response = await _categoryApiService.CreateAsync(request);

            _categoryIdsByName[name] = response.Id;
            AvailableCategories.Add(name);
            SelectedCategory = name;
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(string? categoryName)
    {
        var target = categoryName ?? SelectedCategory;
        if (string.IsNullOrWhiteSpace(target)) return;
        if (!_categoryIdsByName.TryGetValue(target, out var categoryId)) return;

        bool confirmed = await Shell.Current.CurrentPage.DisplayAlert(
            "Kategoriyi Sil",
            $"\"{target}\" kategorisini silmek istediğinize emin misiniz?",
            "Sil", "Vazgeç");

        if (!confirmed) return;

        try
        {
            await _categoryApiService.DeleteAsync(categoryId);
            AvailableCategories.Remove(target);
            _categoryIdsByName.Remove(target);

            if (SelectedCategory == target)
            {
                SelectedCategory = AvailableCategories.FirstOrDefault() ?? DefaultCategory;
            }
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
    }
    [RelayCommand]
private async Task RenameCategoryAsync(string? categoryName)
{
    var target = categoryName ?? SelectedCategory;

    if (string.IsNullOrWhiteSpace(target))
        return;

    if (!_categoryIdsByName.TryGetValue(target, out var id))
        return;

    var newName = await Shell.Current.DisplayPromptAsync(
        "Kategoriyi Yeniden Adlandır",
        "Yeni adı girin:",
        "Kaydet",
        "Vazgeç",
        initialValue: target);

    if (string.IsNullOrWhiteSpace(newName) || newName == target)
        return;

    var vaultKey =  _vaultSessionService.VaultKey;

    if (vaultKey is null)
        return;

    try
    {
        var request = _categoryMapper.ToUpdateRequest(newName, vaultKey);

        await _categoryApiService.UpdateAsync(id, request);

        _categoryIdsByName.Remove(target);
        _categoryIdsByName[newName] = id;

        var index = AvailableCategories.IndexOf(target);

        if (index >= 0)
            AvailableCategories[index] = newName;

        if (SelectedCategory == target)
            SelectedCategory = newName;
    }
    catch (ApiException ex)
    {
        ErrorMessage = ex.Message;
    }
}
}