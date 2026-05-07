using Microsoft.Extensions.Options;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Push;

public sealed class ApnsPushSender(IOptions<ApnsOptions> options, ILogger<ApnsPushSender> logger) : IPushSender
{
    private readonly ApnsOptions _options = options.Value;

    public DevicePlatform Platform => DevicePlatform.Ios;

    public Task<bool> SendAsync(string deviceToken, PushNotification notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.KeyPath) || string.IsNullOrWhiteSpace(_options.TeamId))
        {
            logger.LogInformation(
                "[APNs stub] Would send to {Token}: {Title} - {Body}. Configure Apns:KeyPath, KeyId, TeamId, BundleId in User Secrets to enable real delivery.",
                Mask(deviceToken), notification.Title, notification.Body);
            return Task.FromResult(false);
        }

        logger.LogWarning("APNs delivery is not yet implemented for token {Token}. Add a JWT-signed HTTP/2 client (or a NuGet APNs library) and replace this stub.", Mask(deviceToken));
        return Task.FromResult(false);
    }

    private static string Mask(string token) => token.Length > 8 ? token[..6] + "…" : "<short>";
}
