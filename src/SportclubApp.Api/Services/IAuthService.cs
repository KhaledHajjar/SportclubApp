using SportclubApp.Shared.Auth;

namespace SportclubApp.Api.Services;

public interface IAuthService
{
    Task<AuthOutcome> RegisterAsync(RegisterRequest request, CancellationToken ct);

    Task<AuthOutcome> LoginAsync(LoginRequest request, CancellationToken ct);

    Task<AuthOutcome> RefreshAsync(string refreshToken, CancellationToken ct);

    Task<bool> LogoutAsync(string refreshToken, CancellationToken ct);
}

public sealed record AuthOutcome(bool Success, AuthResponse? Response, string? Error)
{
    public static AuthOutcome Ok(AuthResponse response) => new(true, response, null);

    public static AuthOutcome Fail(string error) => new(false, null, error);
}
