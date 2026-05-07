using SportclubApp.Api.Services.Push;
using SportclubApp.Shared.Push;

namespace SportclubApp.Api.Common.Events.Handlers;

public sealed class SlotOpenedPushHandler(IPushNotificationDispatcher dispatcher) : IDomainEventHandler<SlotOpenedEvent>
{
    public async Task HandleAsync(SlotOpenedEvent @event, CancellationToken ct)
    {
        var notification = new PushNotification(
            Title: "A spot just opened",
            Body: $"You moved off the waiting list — your class starts {@event.ClassStartUtc:f}.",
            Data: new Dictionary<string, string>
            {
                [PushPayloadKeys.Type] = PushNotificationTypes.SlotOpened,
                [PushPayloadKeys.ClassSessionId] = @event.ClassSessionId.ToString(),
                [PushPayloadKeys.ReservationId] = @event.PromotedReservationId.ToString(),
            });

        await dispatcher.DispatchAsync(@event.PromotedMemberId, notification, ct);
    }
}
