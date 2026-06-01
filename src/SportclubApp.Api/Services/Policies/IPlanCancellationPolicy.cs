using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services.Policies;

public interface IPlanCancellationPolicy
{
    PlanTier AppliesTo { get; }

    TimeSpan CancellationLockout { get; }
}
