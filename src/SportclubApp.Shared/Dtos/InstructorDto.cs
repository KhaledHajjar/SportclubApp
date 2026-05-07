namespace SportclubApp.Shared.Dtos;

public sealed record InstructorDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Bio);
