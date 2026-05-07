namespace SportclubApp.Shared.Dtos;

public sealed record WorkoutDto(
    Guid Id,
    string Name,
    string Description,
    int DurationMinutes);
