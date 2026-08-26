using CommunityToolkit.Mvvm.ComponentModel;

namespace PasswordManager.Client.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string WelcomeMessage{get;set;} = "Hoşgeldiniz";


}