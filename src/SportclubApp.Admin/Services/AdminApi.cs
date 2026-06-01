using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Dtos.Admin;

namespace SportclubApp.Admin.Services;

// Typed HttpClient that talks to the API. Bearer attachment + 401-refresh-retry
// is done INSIDE this class rather than via a DelegatingHandler because
// IHttpClientFactory resolves handlers from its own DI scope, not the Blazor
// Server circuit's scope — the handler would see a fresh empty AdminAuthState
// on every call, never the one the Login page populated. AdminApi itself is
// safe to inject AdminAuthState into because it's resolved via @inject inside
// Razor components, which runs through the circuit's scope.
public sealed class AdminApi(HttpClient http, AdminAuthState authState) : IAdminApi
{
    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
        PostJsonAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request, authenticated: false, ct);

    public Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default) =>
        PostJsonAsync<RefreshRequest, AuthResponse>("api/v1/auth/refresh", request, authenticated: false, ct);

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/logout")
        {
            Content = JsonContent.Create(request),
        };
        using var response = await http.SendAsync(msg, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public Task<AdminStatsDto> GetStatsAsync(CancellationToken ct = default) =>
        GetAsync<AdminStatsDto>("api/v1/admin/stats", ct);

    public Task<IReadOnlyList<MemberAdminDto>> GetMembersAsync(string? search, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/v1/admin/members"
            : $"api/v1/admin/members?search={Uri.EscapeDataString(search)}";
        return GetAsync<IReadOnlyList<MemberAdminDto>>(url, ct);
    }

    public Task<IReadOnlyList<PlanAdminDto>> GetPlansAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<PlanAdminDto>>("api/v1/admin/plans", ct);

    public Task<IReadOnlyList<ClassSessionDto>> GetClassSessionsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var url = $"api/v1/admin/class-sessions?from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";
        return GetAsync<IReadOnlyList<ClassSessionDto>>(url, ct);
    }

    public Task<IReadOnlyList<ReservationAdminDto>> GetReservationsAsync(int limit, CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<ReservationAdminDto>>($"api/v1/admin/reservations?limit={limit}", ct);

    // ---- internals ----

    private async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await SendWithAuthAsync(() => new HttpRequestMessage(HttpMethod.Get, path), ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(ct)
            ?? throw new InvalidOperationException($"Empty response from {path}.");
    }

    private async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, bool authenticated, CancellationToken ct)
    {
        HttpRequestMessage Build() => new(HttpMethod.Post, path) { Content = JsonContent.Create(body) };

        using var response = authenticated
            ? await SendWithAuthAsync(Build, ct)
            : await http.SendAsync(Build(), ct);

        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException($"Empty response from {path}.");
    }

    // Attaches Bearer + retries once on 401 by refreshing the token. The request
    // is built fresh each attempt because HttpRequestMessage is single-use.
    private async Task<HttpResponseMessage> SendWithAuthAsync(Func<HttpRequestMessage> requestFactory, CancellationToken ct)
    {
        var request = requestFactory();
        AttachBearer(request);
        var response = await http.SendAsync(request, ct);
        request.Dispose();

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        if (string.IsNullOrEmpty(authState.RefreshToken))
        {
            return response;
        }

        response.Dispose();

        if (!await TryRefreshAsync(ct))
        {
            // Refresh failed — return the unauthenticated attempt; pages will surface the error.
            var noAuth = requestFactory();
            return await http.SendAsync(noAuth, ct);
        }

        var retry = requestFactory();
        AttachBearer(retry);
        return await http.SendAsync(retry, ct);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        try
        {
            var refresh = await RefreshAsync(new RefreshRequest(authState.RefreshToken!), ct);
            authState.UpdateTokens(refresh.AccessToken, refresh.RefreshToken);
            return true;
        }
        catch
        {
            authState.Clear();
            return false;
        }
    }

    private void AttachBearer(HttpRequestMessage message)
    {
        if (!string.IsNullOrEmpty(authState.AccessToken))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.AccessToken);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetailsResponse? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(ct);
        }
        catch
        {
            // Body wasn't ProblemDetails — fall through.
        }

        throw new AdminApiException(response.StatusCode, problem?.Detail, problem?.Title);
    }
}

public sealed class AdminApiException(HttpStatusCode statusCode, string? detail, string? title)
    : Exception(detail ?? title ?? $"API call failed with status {(int)statusCode} {statusCode}.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

internal sealed record ProblemDetailsResponse(string? Type, string? Title, string? Detail, int? Status);
