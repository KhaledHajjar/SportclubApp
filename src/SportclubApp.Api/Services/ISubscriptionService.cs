using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDto?> GetCurrentAsync(Guid memberId, CancellationToken ct);
}
