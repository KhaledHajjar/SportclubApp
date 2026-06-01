using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Entities;

public sealed class Plan
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public PlanTier Tier { get; set; }

    public BillingPeriod BillingPeriod { get; set; }

    public int DurationDays { get; set; }

    public decimal Price { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
