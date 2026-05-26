using System.ComponentModel;
using SportclubApp.Maui.Services.Notifications;
using SportclubApp.Maui.Views;

namespace SportclubApp.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
        Routing.RegisterRoute("schedule", typeof(SchedulePage));
        Routing.RegisterRoute("myReservations", typeof(MyReservationsPage));
        Routing.RegisterRoute("classDetail", typeof(ClassDetailPage));
        Routing.RegisterRoute("history", typeof(HistoryPage));
        Routing.RegisterRoute("instructorClasses", typeof(InstructorClassesPage));
        Routing.RegisterRoute("instructorParticipants", typeof(InstructorParticipantsPage));
        Routing.RegisterRoute("notifications", typeof(NotificationsPage));

        // MAUI Shell tab Title bindings often don't refresh after initial render,
        // so push updates to the visible tab manually whenever the unread count changes.
        NotificationContext.Current.PropertyChanged += OnNotificationContextChanged;
        SyncNotificationsTitle();
    }

    private void OnNotificationContextChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NotificationContext.Title)
            || e.PropertyName == nameof(NotificationContext.UnreadCount))
        {
            MainThread.BeginInvokeOnMainThread(SyncNotificationsTitle);
        }
    }

    private void SyncNotificationsTitle()
    {
        NotificationsTab.Title = NotificationContext.Current.Title;
    }
}
