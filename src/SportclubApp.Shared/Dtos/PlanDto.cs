using SportclubApp.Shared.Enums;

namespace SportclubApp.Shared.Dtos;

public sealed record PlanDto(
    Guid Id,
    string Name,
    PlanTier Tier,
    BillingPeriod BillingPeriod,
    int DurationDays,
    decimal Price);
