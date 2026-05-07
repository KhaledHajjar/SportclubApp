using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Auth;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class HomeViewModel(
    ISportclubApi api,
    ISecureTokenStore tokenStore,
    INavigationService navigation) : BaseViewModel
{
    public HomeViewModel ConfigureTitle()
    {
        Title = "Welcome";
        return this;
    }

    [ObservableProperty]
    public partial string Greeting { get; set; } = "You're signed in.";

    [RelayCommand]
    private async Task SignOutAsync()
    {
        IsBusy = true;
        try
        {
            var refreshToken = await tokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    await api.LogoutAsync(new LogoutRequest(refreshToken));
                }
                catch
                {
                    // Ignore network errors on logout — clear local tokens regardless.
                }
            }
        }
        finally
        {
            await tokenStore.ClearAsync();
            IsBusy = false;
            await navigation.GoToAsync("//login");
        }
    }
}
