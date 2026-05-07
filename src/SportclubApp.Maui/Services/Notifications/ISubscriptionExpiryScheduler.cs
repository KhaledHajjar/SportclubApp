using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.Services.Notifications;

public interface ISubscriptionExpiryScheduler
{
    Task EnsureScheduledAsync(SubscriptionDto? subscription);

    Task CancelAsync();
}
