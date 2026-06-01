namespace SportclubApp.Shared.Dtos;

public sealed record SubscriptionDto(
    Guid Id,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    bool IsActive,
    PlanDto Plan);
