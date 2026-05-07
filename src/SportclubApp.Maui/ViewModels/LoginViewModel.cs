using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class LoginViewModel : BaseViewModel
{
    public LoginViewModel()
    {
        Title = "Sign in";
    }

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [RelayCommand]
    private Task SignInAsync()
    {
        // Implemented in B2.
        return Task.CompletedTask;
    }
}
