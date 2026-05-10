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

    Task<Stream?> GetMyPhotoAsync(CancellationToken ct = default);

    Task<SubscriptionDto?> GetMySubscriptionAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ClassSessionDto>> GetScheduleAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    Task<ClassSessionDto?> GetClassAsync(Guid classId, CancellationToken ct = default);

    Task<ReservationDto> ReserveAsync(Guid classId, CancellationToken ct = default);

    Task<ReservationDto> CancelReservationAsync(Guid reservationId, CancellationToken ct = default);

    Task<IReadOnlyList<ReservationDto>> GetMyReservationsAsync(CancellationToken ct = default);

    Task<WaitingListEntryDto> JoinWaitingListAsync(Guid classId, CancellationToken ct = default);

    Task<WaitingListEntryDto> LeaveWaitingListAsync(Guid entryId, CancellationToken ct = default);

    Task<IReadOnlyList<WaitingListEntryDto>> GetMyWaitingListAsync(CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceRecordDto>> GetMyAttendanceAsync(int year, CancellationToken ct = default);

    Task<IReadOnlyList<ClassSessionDto>> GetInstructorClassesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ClassParticipantDto>> GetClassParticipantsAsync(Guid classId, CancellationToken ct = default);

    Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(bool includeRead, CancellationToken ct = default);

    Task<UnreadCountDto> GetUnreadNotificationsCountAsync(CancellationToken ct = default);

    Task MarkNotificationReadAsync(Guid notificationId, CancellationToken ct = default);

    Task MarkAllNotificationsReadAsync(CancellationToken ct = default);
}
