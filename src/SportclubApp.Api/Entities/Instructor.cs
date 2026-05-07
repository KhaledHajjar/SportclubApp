namespace SportclubApp.Api.Entities;

public sealed class Instructor
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }

    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }

    public ICollection<ClassSession> ClassSessions { get; set; } = [];
}
