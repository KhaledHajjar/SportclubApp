using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Shared.Enums;
using SportclubApp.Shared.Push;

namespace SportclubApp.Api.Services.Push;

public sealed class SubscriptionExpiryNotifier(
    IServiceScopeFactory scopeFactory,
    TimeProvider time,
    ILogger<SubscriptionExpiryNotifier> logger) : BackgroundService
{
    private static readonly TimeSpan WarnBefore = TimeSpan.FromDays(42);
    private static readonly TimeSpan TickInterval = TimeSpan.FromHours(24);
    private readonly HashSet<Guid> _alreadyNotified = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Subscription expiry notifier tick failed.");
            }

            try
            {
                await Task.Delay(TickInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IPushNotificationDispatcher>();

        var now = time.GetUtcNow();
        var threshold = now + WarnBefore;

        var due = await db.Subscriptions
            .Where(s => s.Type == SubscriptionType.Yearly
                        && s.EndUtc > now
                        && s.EndUtc <= threshold)
            .Select(s => new { s.Id, s.MemberId, s.EndUtc })
            .ToListAsync(ct);

        foreach (var sub in due)
        {
            if (!_alreadyNotified.Add(sub.Id))
            {
                continue;
            }

            var notification = new PushNotification(
                Title: "Your subscription expires soon",
                Body: $"Your yearly subscription expires on {sub.EndUtc:d}. Renew to keep going.",
                Data: new Dictionary<string, string>
                {
                    [PushPayloadKeys.Type] = PushNotificationTypes.SubscriptionExpiry,
                    [PushPayloadKeys.SubscriptionId] = sub.Id.ToString(),
                });

            await dispatcher.DispatchAsync(sub.MemberId, notification, ct);
            logger.LogInformation("Sent expiry warning for subscription {SubscriptionId}.", sub.Id);
        }
    }
}
