namespace SportclubApp.Shared.Dtos;

public sealed record UpdateMemberRequest(
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth);
