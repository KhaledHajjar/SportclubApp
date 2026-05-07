using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public sealed class UnlimitedPolicy(SubscriptionType subscriptionType) : ISubscriptionLimitPolicy
{
    public SubscriptionType AppliesTo => subscriptionType;

    public bool CanReserve(int activeWeeklyReservationCount) => true;
}
