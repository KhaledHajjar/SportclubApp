namespace SportclubApp.Shared.Dtos;

public sealed record WaitingListEntryDto(
    Guid Id,
    Guid ClassSessionId,
    int Position,
    DateTimeOffset CreatedUtc);
