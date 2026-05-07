using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public sealed class NotificationService(AppDbContext db, TimeProvider time) : INotificationService
{
    private static readonly IReadOnlyDictionary<string, string> EmptyData =
        new Dictionary<string, string>();

    public async Task CreateAsync(
        Guid memberId,
        string type,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Type = type,
            Title = title,
            Body = body,
            DataJson = data is null || data.Count == 0 ? null : JsonSerializer.Serialize(data),
            CreatedUtc = time.GetUtcNow(),
        };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(Guid memberId, bool includeRead, CancellationToken ct)
    {
        var query = db.Notifications
            .AsNoTracking()
            .Where(n => n.MemberId == memberId);

        if (!includeRead)
        {
            query = query.Where(n => n.ReadUtc == null);
        }

        var rows = await query
            .OrderByDescending(n => n.CreatedUtc)
            .ToListAsync(ct);

        return rows.Select(Map).ToList();
    }

    public Task<int> GetUnreadCountAsync(Guid memberId, CancellationToken ct) =>
        db.Notifications.CountAsync(n => n.MemberId == memberId && n.ReadUtc == null, ct);

    public async Task<bool> MarkAsReadAsync(Guid memberId, Guid notificationId, CancellationToken ct)
    {
        var notification = await db.Notifications
            .SingleOrDefaultAsync(n => n.Id == notificationId && n.MemberId == memberId, ct);
        if (notification is null || notification.ReadUtc is not null)
        {
            return false;
        }

        notification.ReadUtc = time.GetUtcNow();
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid memberId, CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var unread = await db.Notifications
            .Where(n => n.MemberId == memberId && n.ReadUtc == null)
            .ToListAsync(ct);

        foreach (var n in unread)
        {
            n.ReadUtc = now;
        }
        await db.SaveChangesAsync(ct);
        return unread.Count;
    }

    private static NotificationDto Map(Notification n)
    {
        IReadOnlyDictionary<string, string> data = EmptyData;
        if (!string.IsNullOrWhiteSpace(n.DataJson))
        {
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(n.DataJson) ?? new();
            }
            catch (JsonException)
            {
                data = EmptyData;
            }
        }
        return new NotificationDto(n.Id, n.Type, n.Title, n.Body, data, n.CreatedUtc, n.ReadUtc);
    }
}
