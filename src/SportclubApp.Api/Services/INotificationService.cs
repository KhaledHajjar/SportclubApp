using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public interface INotificationService
{
    Task CreateAsync(Guid memberId, string type, string title, string body, IReadOnlyDictionary<string, string>? data, CancellationToken ct);

    Task<IReadOnlyList<NotificationDto>> GetMineAsync(Guid memberId, bool includeRead, CancellationToken ct);

    Task<int> GetUnreadCountAsync(Guid memberId, CancellationToken ct);

    Task<bool> MarkAsReadAsync(Guid memberId, Guid notificationId, CancellationToken ct);

    Task<int> MarkAllAsReadAsync(Guid memberId, CancellationToken ct);
}
