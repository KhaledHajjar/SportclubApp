using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Maui.Services.Notifications;

public sealed class SubscriptionExpiryScheduler : ISubscriptionExpiryScheduler
{
    private const int NotificationId = 1001;
    private static readonly TimeSpan MonthlyWarnBefore = TimeSpan.FromDays(7);
    private static readonly TimeSpan YearlyWarnBefore = TimeSpan.FromDays(42);

    public async Task EnsureScheduledAsync(SubscriptionDto? subscription)
    {
        LocalNotificationCenter.Current.Cancel(NotificationId);

        if (subscription is null)
        {
            return;
        }

        var warnBefore = subscription.Plan.BillingPeriod switch
        {
            BillingPeriod.Yearly => YearlyWarnBefore,
            BillingPeriod.Monthly => MonthlyWarnBefore,
            _ => MonthlyWarnBefore,
        };

        var fireAt = subscription.EndUtc - warnBefore;
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
            Description = $"Your subscription expires on {subscription.EndUtc.LocalDateTime:d}. Renew to keep going.",
            Schedule =
            {
                NotifyTime = fireAt,
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
