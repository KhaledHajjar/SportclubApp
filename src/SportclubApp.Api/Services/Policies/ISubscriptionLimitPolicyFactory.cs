using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public interface ISubscriptionLimitPolicyFactory
{
    ISubscriptionLimitPolicy GetFor(SubscriptionType type);
}

public sealed class SubscriptionLimitPolicyFactory : ISubscriptionLimitPolicyFactory
{
    public ISubscriptionLimitPolicy GetFor(SubscriptionType type) => type switch
    {
        SubscriptionType.TwicePerWeek => new TwicePerWeekPolicy(),
        SubscriptionType.Yearly => new UnlimitedPolicy(SubscriptionType.Yearly),
        SubscriptionType.Unlimited => new UnlimitedPolicy(SubscriptionType.Unlimited),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown subscription type."),
    };
}
