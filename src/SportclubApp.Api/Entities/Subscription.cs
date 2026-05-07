using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Entities;

public sealed class Subscription
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public SubscriptionType Type { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
}
