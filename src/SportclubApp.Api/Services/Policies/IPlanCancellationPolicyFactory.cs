using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public interface IPlanCancellationPolicyFactory
{
    IPlanCancellationPolicy GetFor(PlanTier tier);
}

public sealed class PlanCancellationPolicyFactory : IPlanCancellationPolicyFactory
{
    public IPlanCancellationPolicy GetFor(PlanTier tier) => tier switch
    {
        PlanTier.Standard => new StandardPlanPolicy(),
        PlanTier.Premium => new PremiumPlanPolicy(),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown plan tier."),
    };
}
