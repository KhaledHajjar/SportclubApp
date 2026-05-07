namespace SportclubApp.Maui.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        return parameters is null
            ? Shell.Current.GoToAsync(route)
            : Shell.Current.GoToAsync(route, parameters);
    }

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public Task DisplayAlertAsync(string title, string message, string cancel = "OK") =>
        Shell.Current.DisplayAlert(title, message, cancel);

    public Task<bool> DisplayConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No") =>
        Shell.Current.DisplayAlert(title, message, accept, cancel);
}
