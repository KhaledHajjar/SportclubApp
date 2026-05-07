namespace SportclubApp.Api.Common.Events;

public sealed record SlotOpenedEvent(
    Guid ClassSessionId,
    Guid PromotedMemberId,
    Guid PromotedReservationId,
    DateTimeOffset ClassStartUtc);
