namespace SportclubApp.Api.Entities;

public sealed class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }

    public ICollection<ClassSession> ClassSessions { get; set; } = [];
}
