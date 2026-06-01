using SportclubApp.Shared.Enums;

namespace SportclubApp.Shared.Dtos.Admin;

public sealed record PlanAdminDto(
    Guid Id,
    string Name,
    PlanTier Tier,
    BillingPeriod BillingPeriod,
    int DurationDays,
    decimal Price,
    int ActiveSubscriptionCount);
