using SportclubApp.Maui.Views;

namespace SportclubApp.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("home", typeof(HomePage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
    }
}
