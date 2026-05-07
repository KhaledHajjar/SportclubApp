namespace SportclubApp.Shared.Auth;

public sealed record AuthResponse(
    Guid MemberId,
    string Email,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresUtc,
    IReadOnlyList<string> Roles);
