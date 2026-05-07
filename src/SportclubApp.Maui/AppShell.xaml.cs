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
        Routing.RegisterRoute("schedule", typeof(SchedulePage));
        Routing.RegisterRoute("myReservations", typeof(MyReservationsPage));
        Routing.RegisterRoute("classDetail", typeof(ClassDetailPage));
        Routing.RegisterRoute("history", typeof(HistoryPage));
        Routing.RegisterRoute("instructorClasses", typeof(InstructorClassesPage));
        Routing.RegisterRoute("instructorParticipants", typeof(InstructorParticipantsPage));
        Routing.RegisterRoute("notifications", typeof(NotificationsPage));
    }
}
