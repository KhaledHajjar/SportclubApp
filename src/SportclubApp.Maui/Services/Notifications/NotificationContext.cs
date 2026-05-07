using CommunityToolkit.Mvvm.ComponentModel;

namespace SportclubApp.Maui.Services.Notifications;

public sealed partial class NotificationContext : ObservableObject
{
    public static NotificationContext Current { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(HasUnread))]
    private int _unreadCount;

    public string Title => UnreadCount > 0 ? $"Notifications ({UnreadCount})" : "Notifications";

    public bool HasUnread => UnreadCount > 0;
}
