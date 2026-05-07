using SportclubApp.Api.Services;
using SportclubApp.Shared.Notifications;

namespace SportclubApp.Api.Common.Events.Handlers;

public sealed class SlotOpenedNotificationHandler(INotificationService notifications)
    : IDomainEventHandler<SlotOpenedEvent>
{
    public Task HandleAsync(SlotOpenedEvent @event, CancellationToken ct) =>
        notifications.CreateAsync(
            memberId: @event.PromotedMemberId,
            type: NotificationTypes.SlotOpened,
            title: "A spot just opened",
            body: $"You moved off the waiting list — your class starts {@event.ClassStartUtc:f}.",
            data: new Dictionary<string, string>
            {
                [NotificationDataKeys.ClassSessionId] = @event.ClassSessionId.ToString(),
                [NotificationDataKeys.ReservationId] = @event.PromotedReservationId.ToString(),
            },
            ct: ct);
}
