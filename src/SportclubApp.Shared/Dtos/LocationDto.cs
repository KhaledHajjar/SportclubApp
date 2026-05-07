namespace SportclubApp.Shared.Dtos;

public sealed record LocationDto(
    Guid Id,
    string Name,
    string? Address);
