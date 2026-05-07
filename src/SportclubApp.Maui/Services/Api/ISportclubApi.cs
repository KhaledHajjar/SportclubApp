using SportclubApp.Shared.Auth;

namespace SportclubApp.Maui.Services.Api;

public interface ISportclubApi
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);
}
