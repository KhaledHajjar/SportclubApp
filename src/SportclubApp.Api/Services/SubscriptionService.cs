using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public sealed class SubscriptionService(AppDbContext db) : ISubscriptionService
{
    public async Task<SubscriptionDto?> GetCurrentAsync(Guid memberId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var subscription = await db.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.MemberId == memberId && s.StartUtc <= now && s.EndUtc > now)
            .OrderByDescending(s => s.EndUtc)
            .FirstOrDefaultAsync(ct);

        if (subscription is null)
        {
            return null;
        }

        return new SubscriptionDto(
            Id: subscription.Id,
            StartUtc: subscription.StartUtc,
            EndUtc: subscription.EndUtc,
            IsActive: true,
            Plan: new PlanDto(
                subscription.Plan.Id,
                subscription.Plan.Name,
                subscription.Plan.Tier,
                subscription.Plan.BillingPeriod,
                subscription.Plan.DurationDays,
                subscription.Plan.Price));
    }
}
