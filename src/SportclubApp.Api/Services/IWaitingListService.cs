using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public interface IWaitingListService
{
    Task<WaitingListResult> JoinAsync(Guid memberId, Guid classSessionId, CancellationToken ct);

    Task<WaitingListResult> LeaveAsync(Guid memberId, Guid entryId, CancellationToken ct);

    Task<IReadOnlyList<WaitingListEntryDto>> GetMineAsync(Guid memberId, CancellationToken ct);
}

public sealed record WaitingListResult(
    bool Success,
    WaitingListEntryDto? Entry,
    string? ErrorType,
    string? ErrorDetail)
{
    public static WaitingListResult Ok(WaitingListEntryDto entry) => new(true, entry, null, null);

    public static WaitingListResult Fail(string errorType, string detail) => new(false, null, errorType, detail);
}
