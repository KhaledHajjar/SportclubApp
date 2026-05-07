namespace SportclubApp.Shared.Dtos;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    IReadOnlyDictionary<string, string> Data,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? ReadUtc);

public sealed record UnreadCountDto(int Unread);
