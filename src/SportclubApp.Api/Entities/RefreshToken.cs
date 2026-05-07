namespace SportclubApp.Api.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresUtc { get; set; }
    public DateTimeOffset? RevokedUtc { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsRevoked => RevokedUtc is not null;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresUtc;
    public bool IsActive => !IsRevoked && !IsExpired;
}
