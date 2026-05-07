using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Push;

public sealed class FcmPushSender : IPushSender
{
    private readonly ILogger<FcmPushSender> _logger;
    private readonly FirebaseMessaging? _messaging;

    public FcmPushSender(IOptions<FcmOptions> options, ILogger<FcmPushSender> logger)
    {
        _logger = logger;

        var path = options.Value.ServiceAccountJsonPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _logger.LogWarning("FCM is not configured (Fcm:ServiceAccountJsonPath missing or file not found). Push delivery for Android will be skipped.");
            return;
        }

        var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(path),
        });
        _messaging = FirebaseMessaging.GetMessaging(app);
    }

    public DevicePlatform Platform => DevicePlatform.Android;

    public async Task<bool> SendAsync(string deviceToken, PushNotification notification, CancellationToken ct)
    {
        if (_messaging is null)
        {
            _logger.LogInformation("[FCM stub] Would send to {Token}: {Title} - {Body}", Mask(deviceToken), notification.Title, notification.Body);
            return false;
        }

        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body,
            },
            Data = new Dictionary<string, string>(notification.Data),
        };

        try
        {
            var id = await _messaging.SendAsync(message, ct);
            _logger.LogInformation("FCM message sent: {MessageId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send FCM message to {Token}.", Mask(deviceToken));
            return false;
        }
    }

    private static string Mask(string token) => token.Length > 8 ? token[..6] + "…" : "<short>";
}
