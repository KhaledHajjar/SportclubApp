namespace SportclubApp.Api.Common.Events.Handlers;

public sealed class SlotOpenedEventLoggingHandler(ILogger<SlotOpenedEventLoggingHandler> logger)
    : IDomainEventHandler<SlotOpenedEvent>
{
    public Task HandleAsync(SlotOpenedEvent @event, CancellationToken ct)
    {
        logger.LogInformation(
            "Slot opened: class {ClassSessionId} - promoted member {MemberId} via reservation {ReservationId}, class starts {StartUtc:o}.",
            @event.ClassSessionId, @event.PromotedMemberId, @event.PromotedReservationId, @event.ClassStartUtc);
        return Task.CompletedTask;
    }
}
