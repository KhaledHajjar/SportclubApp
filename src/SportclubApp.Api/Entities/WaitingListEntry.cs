namespace SportclubApp.Api.Entities;

public sealed class WaitingListEntry
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;

    public int Position { get; set; }
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
}
