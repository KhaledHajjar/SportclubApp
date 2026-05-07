namespace SportclubApp.Shared.Dtos;

public sealed record ClassSessionDto(
    Guid Id,
    DateTimeOffset StartUtc,
    int Capacity,
    int ReservedCount,
    int WaitingListCount,
    int FreeSpots,
    bool IsFull,
    WorkoutDto Workout,
    InstructorDto Instructor,
    LocationDto Location);
