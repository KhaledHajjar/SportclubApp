using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Auth;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class LoginViewModel(
    ISportclubApi api,
    ISecureTokenStore tokenStore,
    INavigationService navigation) : BaseViewModel
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public LoginViewModel ConfigureTitle()
    {
        Title = "Sign in";
        return this;
    }

    public async Task TryAutoLoginAsync()
    {
        var refreshToken = await tokenStore.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        try
        {
            var response = await api.RefreshAsync(new RefreshRequest(refreshToken));
            await tokenStore.SaveTokensAsync(response.AccessToken, response.RefreshToken);
            UserContext.Current.Apply(response);
            await navigation.GoToAsync("//main");
        }
        catch
        {
            await tokenStore.ClearAsync();
            UserContext.Current.Clear();
        }
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required.";
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var response = await api.LoginAsync(new LoginRequest(Email.Trim(), Password));
            await tokenStore.SaveTokensAsync(response.AccessToken, response.RefreshToken);
            UserContext.Current.Apply(response);
            Password = string.Empty;
            await navigation.GoToAsync("//main");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            ErrorMessage = "Invalid email or password.";
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ErrorMessage = "Email and password are required.";
        }
        catch (Exception)
        {
            ErrorMessage = "Could not reach the server. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
