using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.Services.Api;

public sealed class SportclubApi(HttpClient http) : ISportclubApi
{
    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default) =>
        PostJsonAsync<RegisterRequest, AuthResponse>("api/v1/auth/register", request, ct);

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
        PostJsonAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request, ct);

    public Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default) =>
        PostJsonAsync<RefreshRequest, AuthResponse>("api/v1/auth/refresh", request, ct);

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/v1/auth/logout", request, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public Task<MemberDto> GetMeAsync(CancellationToken ct = default) =>
        GetAsync<MemberDto>("api/v1/members/me", ct);

    public async Task<MemberDto> UpdateMeAsync(UpdateMemberRequest request, CancellationToken ct = default)
    {
        using var response = await http.PutAsJsonAsync("api/v1/members/me", request, ct);
        await EnsureSuccessAsync(response, ct);
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
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<MemberDto>(ct)
            ?? throw new InvalidOperationException("Empty response body from POST /members/me/photo.");
    }

    public async Task<Stream?> GetMyPhotoAsync(CancellationToken ct = default)
    {
        using var response = await http.GetAsync("api/v1/members/me/photo", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        await EnsureSuccessAsync(response, ct);
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        return new MemoryStream(bytes);
    }

    public async Task<SubscriptionDto?> GetMySubscriptionAsync(CancellationToken ct = default)
    {
        using var response = await http.GetAsync("api/v1/subscriptions/me", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<SubscriptionDto>(ct);
    }

    public async Task<IReadOnlyList<ClassSessionDto>> GetScheduleAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var url = $"api/v1/classes?from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";
        return await GetAsync<List<ClassSessionDto>>(url, ct);
    }

    public async Task<ClassSessionDto?> GetClassAsync(Guid classId, CancellationToken ct = default)
    {
        using var response = await http.GetAsync($"api/v1/classes/{classId}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<ClassSessionDto>(ct);
    }

    public async Task<ReservationDto> ReserveAsync(Guid classId, CancellationToken ct = default)
    {
        using var response = await http.PostAsync($"api/v1/classes/{classId}/reservations", content: null, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<ReservationDto>(ct)
            ?? throw new InvalidOperationException("Empty response body from reservation create.");
    }

    public async Task<ReservationDto> CancelReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/v1/reservations/{reservationId}", ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<ReservationDto>(ct)
            ?? throw new InvalidOperationException("Empty response body from reservation cancel.");
    }

    public Task<IReadOnlyList<ReservationDto>> GetMyReservationsAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<ReservationDto>>("api/v1/reservations/me", ct);

    public async Task<WaitingListEntryDto> JoinWaitingListAsync(Guid classId, CancellationToken ct = default)
    {
        using var response = await http.PostAsync($"api/v1/classes/{classId}/waiting-list", content: null, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<WaitingListEntryDto>(ct)
            ?? throw new InvalidOperationException("Empty response body from waiting-list join.");
    }

    public async Task<WaitingListEntryDto> LeaveWaitingListAsync(Guid entryId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/v1/waiting-list/{entryId}", ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<WaitingListEntryDto>(ct)
            ?? throw new InvalidOperationException("Empty response body from waiting-list leave.");
    }

    public Task<IReadOnlyList<WaitingListEntryDto>> GetMyWaitingListAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<WaitingListEntryDto>>("api/v1/waiting-list/me", ct);

    public Task<IReadOnlyList<AttendanceRecordDto>> GetMyAttendanceAsync(int year, CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<AttendanceRecordDto>>($"api/v1/attendance/me?year={year}", ct);

    public Task<IReadOnlyList<ClassSessionDto>> GetInstructorClassesAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<ClassSessionDto>>("api/v1/instructors/me/classes", ct);

    public Task<IReadOnlyList<ClassParticipantDto>> GetClassParticipantsAsync(Guid classId, CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<ClassParticipantDto>>($"api/v1/instructors/me/classes/{classId}/participants", ct);

    public Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(bool includeRead, CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<NotificationDto>>($"api/v1/notifications?includeRead={(includeRead ? "true" : "false")}", ct);

    public Task<UnreadCountDto> GetUnreadNotificationsCountAsync(CancellationToken ct = default) =>
        GetAsync<UnreadCountDto>("api/v1/notifications/unread-count", ct);

    public async Task MarkNotificationReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        using var response = await http.PostAsync($"api/v1/notifications/{notificationId}/read", content: null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task MarkAllNotificationsReadAsync(CancellationToken ct = default)
    {
        using var response = await http.PostAsync("api/v1/notifications/read-all", content: null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private async Task<TResponse> GetAsync<TResponse>(string path, CancellationToken ct)
    {
        using var response = await http.GetAsync(path, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException($"Empty response body from {path}.");
    }

    private async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync(path, body, ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<TResponse>(ct)
            ?? throw new InvalidOperationException($"Empty response body from {path}.");
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
            // Body wasn't JSON / wasn't a ProblemDetails. Fall through.
        }

        throw new ApiException(response.StatusCode, problem?.Type, problem?.Detail, problem?.Title);
    }
}
