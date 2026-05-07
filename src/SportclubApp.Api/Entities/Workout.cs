namespace SportclubApp.Api.Entities;

public sealed class Workout
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    public ICollection<ClassSession> ClassSessions { get; set; } = [];
}
