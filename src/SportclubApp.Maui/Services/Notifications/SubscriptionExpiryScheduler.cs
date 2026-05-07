using Plugin.LocalNotification;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Maui.Services.Notifications;

public sealed class SubscriptionExpiryScheduler : ISubscriptionExpiryScheduler
{
    private const int NotificationId = 1001;
    private static readonly TimeSpan WarnBefore = TimeSpan.FromDays(42);

    public async Task EnsureScheduledAsync(SubscriptionDto? subscription)
    {
        LocalNotificationCenter.Current.Cancel(NotificationId);

        if (subscription is null || subscription.Type != SubscriptionType.Yearly)
        {
            return;
        }

        var fireAt = subscription.EndUtc - WarnBefore;
        if (fireAt <= DateTimeOffset.UtcNow)
        {
            return;
        }

        var permission = await LocalNotificationCenter.Current.RequestNotificationPermission();
        if (!permission)
        {
            return;
        }

        var request = new NotificationRequest
        {
            NotificationId = NotificationId,
            Title = "Your subscription expires soon",
            Description = $"Your yearly subscription expires on {subscription.EndUtc.LocalDateTime:d}. Renew to keep going.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = fireAt.LocalDateTime,
            },
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public Task CancelAsync()
    {
        LocalNotificationCenter.Current.Cancel(NotificationId);
        return Task.CompletedTask;
    }
}
