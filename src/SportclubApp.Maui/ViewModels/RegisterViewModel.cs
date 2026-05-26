using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Auth;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Maui.Services.Notifications;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class RegisterViewModel(
    ISportclubApi api,
    ISecureTokenStore tokenStore,
    INavigationService navigation,
    ISubscriptionExpiryScheduler expiryScheduler,
    INotificationsBadgeService badge) : BaseViewModel
{
    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public RegisterViewModel ConfigureTitle()
    {
        Title = "Create account";
        return this;
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName)
            || string.IsNullOrWhiteSpace(LastName)
            || string.IsNullOrWhiteSpace(Email)
            || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var response = await api.RegisterAsync(new RegisterRequest(
                Email: Email.Trim(),
                Password: Password,
                FirstName: FirstName.Trim(),
                LastName: LastName.Trim()));

            await tokenStore.SaveTokensAsync(response.AccessToken, response.RefreshToken);
            UserContext.Current.Apply(response);
            Password = string.Empty;

            await ScheduleSubscriptionExpiryAsync();
            await badge.RefreshAsync();
            await navigation.GoToAsync("//main");
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            ErrorMessage = "An account with this email already exists.";
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ErrorMessage = ex.Detail ?? "Some fields are invalid. Please check and try again.";
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

    [RelayCommand]
    private Task BackToSignInAsync() => navigation.GoBackAsync();

    private async Task ScheduleSubscriptionExpiryAsync()
    {
        try
        {
            var subscription = await api.GetMySubscriptionAsync();
            await expiryScheduler.EnsureScheduledAsync(subscription);
        }
        catch
        {
            // Best-effort — never block sign-up on local-notification scheduling.
        }
    }
}
