using SportclubApp.Shared.Enums;

namespace SportclubApp.Shared.Dtos.Admin;

public sealed record ReservationAdminDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid ClassSessionId,
    string WorkoutName,
    DateTimeOffset ClassStartUtc,
    DateTimeOffset CreatedUtc,
    ReservationStatus Status);
