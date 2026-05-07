using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Entities;

public sealed class Reservation
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;

    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
}
