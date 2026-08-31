using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Client.Models;

namespace PasswordManager.Client.ViewModels;

public partial class VaultItemDetailViewModel : ObservableObject, IQueryAttributable
{

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Username { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsPasswordMasked { get; set; } = true;
    

    [RelayCommand]
    private void ToggleMask(){
        IsPasswordMasked = !IsPasswordMasked;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue("Item", out var value);
        if(value is VaultItemListEntry entry)
        {
            Title = entry.Payload.Title;
            Username = entry.Payload.Username;
            Password = entry.Payload.Password;
            Notes = entry.Payload.Notes;
        }
    }
}