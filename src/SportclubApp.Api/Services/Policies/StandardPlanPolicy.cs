using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public sealed class StandardPlanPolicy : IPlanCancellationPolicy
{
    public PlanTier AppliesTo => PlanTier.Standard;

    public TimeSpan CancellationLockout => TimeSpan.FromHours(1);
}
