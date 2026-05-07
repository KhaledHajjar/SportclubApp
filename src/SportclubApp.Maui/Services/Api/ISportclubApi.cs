using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.Services.Api;

public interface ISportclubApi
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);

    Task<MemberDto> GetMeAsync(CancellationToken ct = default);

    Task<MemberDto> UpdateMeAsync(UpdateMemberRequest request, CancellationToken ct = default);

    Task<MemberDto> UploadProfilePhotoAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

    Task<SubscriptionDto?> GetMySubscriptionAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ClassSessionDto>> GetScheduleAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    Task<ClassSessionDto?> GetClassAsync(Guid classId, CancellationToken ct = default);

    Task<ReservationDto> ReserveAsync(Guid classId, CancellationToken ct = default);

    Task<ReservationDto> CancelReservationAsync(Guid reservationId, CancellationToken ct = default);

    Task<IReadOnlyList<ReservationDto>> GetMyReservationsAsync(CancellationToken ct = default);
}
