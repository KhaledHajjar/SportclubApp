namespace SportclubApp.Shared.Dtos;

public sealed record MemberDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    bool HasPhoto);
