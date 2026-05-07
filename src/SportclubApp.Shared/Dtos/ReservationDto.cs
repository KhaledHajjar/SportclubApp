using SportclubApp.Shared.Enums;

namespace SportclubApp.Shared.Dtos;

public sealed record ReservationDto(
    Guid Id,
    Guid ClassSessionId,
    DateTimeOffset ClassStartUtc,
    DateTimeOffset CreatedUtc,
    ReservationStatus Status);
