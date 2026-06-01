namespace SportclubApp.Shared.Dtos.Admin;

public sealed record MemberAdminDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    IReadOnlyList<string> Roles,
    string? ActivePlanName,
    DateTimeOffset? PlanEndsUtc,
    DateTimeOffset CreatedUtc);
