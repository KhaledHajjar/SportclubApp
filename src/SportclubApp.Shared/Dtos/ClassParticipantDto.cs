namespace SportclubApp.Shared.Dtos;

public sealed record ClassParticipantDto(
    Guid MemberId,
    string FirstName,
    string LastName,
    DateTimeOffset ReservedAtUtc);
