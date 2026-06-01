using System.Text.Json.Serialization;

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
    LocationDto Location)
{
    // Client-side helper for capacity bar bindings. JsonIgnore keeps the wire payload lean.
    [JsonIgnore]
    public double CapacityFraction => Capacity > 0
        ? Math.Clamp((double)ReservedCount / Capacity, 0.0, 1.0)
        : 0.0;
}
