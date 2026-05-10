using SportclubApp.Maui.Services.Api;

namespace SportclubApp.Maui.Services.Notifications;

public sealed class NotificationsBadgeService(ISportclubApi api) : INotificationsBadgeService
{
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            var unread = await api.GetUnreadNotificationsCountAsync(ct);
            NotificationContext.Current.UnreadCount = unread.Unread;
        }
        catch
        {
            // Best-effort — badge updates must never block the caller.
        }
    }
}
