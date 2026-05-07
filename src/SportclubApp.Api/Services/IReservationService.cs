using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public interface IReservationService
{
    Task<ReservationResult> ReserveAsync(Guid memberId, Guid classSessionId, CancellationToken ct);

    Task<ReservationResult> CancelAsync(Guid memberId, Guid reservationId, CancellationToken ct);

    Task<IReadOnlyList<ReservationDto>> GetMineAsync(Guid memberId, CancellationToken ct);
}

public sealed record ReservationResult(
    bool Success,
    ReservationDto? Reservation,
    string? ErrorType,
    string? ErrorDetail)
{
    public static ReservationResult Ok(ReservationDto reservation) => new(true, reservation, null, null);

    public static ReservationResult Fail(string errorType, string detail) => new(false, null, errorType, detail);
}
