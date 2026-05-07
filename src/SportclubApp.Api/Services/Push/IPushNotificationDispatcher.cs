namespace SportclubApp.Api.Services.Push;

public interface IPushNotificationDispatcher
{
    Task DispatchAsync(Guid memberId, PushNotification notification, CancellationToken ct);
}
