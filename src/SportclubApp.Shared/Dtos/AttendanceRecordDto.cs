namespace SportclubApp.Shared.Dtos;

public sealed record AttendanceRecordDto(
    Guid Id,
    Guid ClassSessionId,
    DateTimeOffset AttendedUtc,
    string WorkoutName);
