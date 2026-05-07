using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Push;

public interface IPushSender
{
    DevicePlatform Platform { get; }

    Task<bool> SendAsync(string deviceToken, PushNotification notification, CancellationToken ct);
}
