namespace SportclubApp.Maui.Services.Notifications;

public interface INotificationsBadgeService
{
    Task RefreshAsync(CancellationToken ct = default);
}
