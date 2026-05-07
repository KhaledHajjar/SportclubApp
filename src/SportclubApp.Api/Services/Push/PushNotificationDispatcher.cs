using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;

namespace SportclubApp.Api.Services.Push;

public sealed class PushNotificationDispatcher(
    AppDbContext db,
    IEnumerable<IPushSender> senders,
    ILogger<PushNotificationDispatcher> logger) : IPushNotificationDispatcher
{
    private readonly Dictionary<Shared.Enums.DevicePlatform, IPushSender> _byPlatform = senders.ToDictionary(s => s.Platform);

    public async Task DispatchAsync(Guid memberId, PushNotification notification, CancellationToken ct)
    {
        var devices = await db.DeviceTokens
            .AsNoTracking()
            .Where(d => d.MemberId == memberId)
            .Select(d => new { d.Token, d.Platform })
            .ToListAsync(ct);

        if (devices.Count == 0)
        {
            logger.LogInformation("Member {MemberId} has no registered devices; skipping push.", memberId);
            return;
        }

        foreach (var device in devices)
        {
            if (!_byPlatform.TryGetValue(device.Platform, out var sender))
            {
                logger.LogWarning("No push sender registered for platform {Platform}.", device.Platform);
                continue;
            }

            await sender.SendAsync(device.Token, notification, ct);
        }
    }
}
