namespace SportclubApp.Maui.Services.Navigation;

public interface INavigationService
{
    Task GoToAsync(string route, IDictionary<string, object>? parameters = null);

    Task GoBackAsync();

    Task DisplayAlertAsync(string title, string message, string cancel = "OK");

    Task<bool> DisplayConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");
}
