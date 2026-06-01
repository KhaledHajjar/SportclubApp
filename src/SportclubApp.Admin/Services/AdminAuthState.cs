using SportclubApp.Shared.Auth;

namespace SportclubApp.Admin.Services;

// Scoped service: one instance per Blazor Server circuit (per signed-in user).
// Holds the JWT pair in memory for the duration of the SignalR connection.
public sealed class AdminAuthState
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public Guid? MemberId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; private set; } = Array.Empty<string>();

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);
    public bool IsAdmin => Roles.Contains(AuthRoles.Admin);

    public event Action? StateChanged;

    public void Apply(AuthResponse response)
    {
        AccessToken = response.AccessToken;
        RefreshToken = response.RefreshToken;
        MemberId = response.MemberId;
        DisplayName = response.Email;
        Roles = response.Roles;
        StateChanged?.Invoke();
    }

    // Called by the delegating handler after a silent refresh.
    public void UpdateTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        MemberId = null;
        DisplayName = string.Empty;
        Roles = Array.Empty<string>();
        StateChanged?.Invoke();
    }
}
