using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;

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

    public Task<MemberDto> GetMeAsync(CancellationToken ct = default) =>
        GetAsync<MemberDto>("api/v1/members/me", ct);

    public async Task<MemberDto> UpdateMeAsync(UpdateMemberRequest request, CancellationToken ct = default)
    {
        using var response = await http.PutAsJsonAsync("api/v1/members/me", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MemberDto>(ct)
            ?? throw new InvalidOperationException("Empty response body from PUT /members/me.");
    }

    public async Task<MemberDto> UploadProfilePhotoAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);

        using var response = await http.PostAsync("api/v1/members/me/photo", form, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MemberDto>(ct)
            ?? throw new InvalidOperationException("Empty response body from POST /members/me/photo.");
    }

    public async Task<SubscriptionDto?> GetMySubscriptionAsync(CancellationToken ct = default)
    {
        using var response = await http.GetAsync("api/v1/subscriptions/me", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SubscriptionDto>(ct);
    }

    public async Task<IReadOnlyList<ClassSessionDto>> GetScheduleAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var url = $"api/v1/classes?from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";
        var result = await GetAsync<List<ClassSessionDto>>(url, ct);
        return result;
    }

    public async Task<ClassSessionDto?> GetClassAsync(Guid classId, CancellationToken ct = default)
    {
        using var response = await http.GetAsync($"api/v1/classes/{classId}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ClassSessionDto>(ct);
    }

    private async Task<TResponse> GetAsync<TResponse>(string path, CancellationToken ct)
    {
        using var response = await http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException($"Empty response body from {path}.");
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync(path, body, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException($"Empty response body from {path}.");
    }
}
