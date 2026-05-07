using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public sealed class TwicePerWeekPolicy : ISubscriptionLimitPolicy
{
    public const int WeeklyVisitLimit = 2;

    public SubscriptionType AppliesTo => SubscriptionType.TwicePerWeek;

    public bool CanReserve(int activeWeeklyReservationCount) => activeWeeklyReservationCount < WeeklyVisitLimit;
}
