namespace SportclubApp.Api.Entities;

public sealed class ClassSession
{
    public Guid Id { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public int Capacity { get; set; }

    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;

    public Guid InstructorId { get; set; }
    public Instructor Instructor { get; set; } = null!;

    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public ICollection<Reservation> Reservations { get; set; } = [];
    public ICollection<WaitingListEntry> WaitingList { get; set; } = [];
}
