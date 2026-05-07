using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services;

public sealed class SubscriptionService(AppDbContext db) : ISubscriptionService
{
    public async Task<SubscriptionDto?> GetCurrentAsync(Guid memberId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var subscription = await db.Subscriptions
            .Where(s => s.MemberId == memberId && s.StartUtc <= now && s.EndUtc > now)
            .OrderByDescending(s => s.EndUtc)
            .FirstOrDefaultAsync(ct);

        if (subscription is null)
        {
            return null;
        }

        int? remainingWeeklyVisits = null;
        if (subscription.Type == SubscriptionType.TwicePerWeek)
        {
            var (weekStart, weekEnd) = CurrentIsoWeek(now);

            var visitsThisWeek = await db.Reservations
                .Where(r => r.MemberId == memberId
                            && r.Status == ReservationStatus.Active
                            && r.ClassSession.StartUtc >= weekStart
                            && r.ClassSession.StartUtc < weekEnd)
                .CountAsync(ct);

            remainingWeeklyVisits = Math.Max(0, 2 - visitsThisWeek);
        }

        return new SubscriptionDto(
            Id: subscription.Id,
            Type: subscription.Type,
            StartUtc: subscription.StartUtc,
            EndUtc: subscription.EndUtc,
            IsActive: true,
            RemainingWeeklyVisits: remainingWeeklyVisits);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) CurrentIsoWeek(DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var monday = today.AddDays(-daysSinceMonday);
        var nextMonday = monday.AddDays(7);

        return (
            new DateTimeOffset(monday.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new DateTimeOffset(nextMonday.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
    }
}
