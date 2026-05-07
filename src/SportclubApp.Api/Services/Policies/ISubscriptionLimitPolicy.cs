using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public interface ISubscriptionLimitPolicy
{
    SubscriptionType AppliesTo { get; }

    bool CanReserve(int activeWeeklyReservationCount);
}
