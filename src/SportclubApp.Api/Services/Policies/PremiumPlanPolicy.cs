using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public sealed class PremiumPlanPolicy : IPlanCancellationPolicy
{
    public PlanTier AppliesTo => PlanTier.Premium;

    public TimeSpan CancellationLockout => TimeSpan.FromMinutes(15);
}
