using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Entities;

public sealed class DeviceToken
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DevicePlatform Platform { get; set; }
    public DateTimeOffset RegisteredUtc { get; set; } = DateTimeOffset.UtcNow;
}
