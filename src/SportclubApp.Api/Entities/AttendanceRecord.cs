namespace SportclubApp.Api.Entities;

public sealed class AttendanceRecord
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;

    public DateTimeOffset AttendedUtc { get; set; }
}
