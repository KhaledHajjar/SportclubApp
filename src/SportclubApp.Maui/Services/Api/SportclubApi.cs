using System.Net.Http.Json;
using SportclubApp.Shared.Auth;

namespace SportclubApp.Maui.Services.Api;

public sealed class SportclubApi(HttpClient http) : ISportclubApi
{
    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default) =>
        PostAsync<RegisterRequest, AuthResponse>("api/v1/auth/register", request, ct);

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
        PostAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request, ct);

    public Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default) =>
        PostAsync<RefreshRequest, AuthResponse>("api/v1/auth/refresh", request, ct);

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/v1/auth/logout", request, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync(path, body, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException($"Empty response body from {path}.");
    }
}
