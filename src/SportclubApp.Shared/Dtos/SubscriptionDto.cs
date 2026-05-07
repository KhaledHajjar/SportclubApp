using SportclubApp.Shared.Enums;

namespace SportclubApp.Shared.Dtos;

public sealed record SubscriptionDto(
    Guid Id,
    SubscriptionType Type,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    bool IsActive,
    int? RemainingWeeklyVisits);
