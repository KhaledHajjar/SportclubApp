namespace SportclubApp.Api.Services;

public interface IWaitingListPromotionService
{
    Task<bool> TryPromoteHeadAsync(Guid classSessionId, CancellationToken ct);
}
