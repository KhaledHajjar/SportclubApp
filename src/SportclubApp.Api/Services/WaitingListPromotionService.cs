using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Common.Events;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services;

public sealed class WaitingListPromotionService(
    AppDbContext db,
    IDomainEventDispatcher events,
    TimeProvider time) : IWaitingListPromotionService
{
    public async Task<bool> TryPromoteHeadAsync(Guid classSessionId, CancellationToken ct)
    {
        var session = await db.ClassSessions.SingleOrDefaultAsync(c => c.Id == classSessionId, ct);
        if (session is null)
        {
            return false;
        }

        var head = await db.WaitingListEntries
            .Where(w => w.ClassSessionId == classSessionId)
            .OrderBy(w => w.Position)
            .FirstOrDefaultAsync(ct);

        if (head is null)
        {
            return false;
        }

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = head.MemberId,
            ClassSessionId = classSessionId,
            CreatedUtc = time.GetUtcNow(),
            Status = ReservationStatus.Active,
        };
        db.Reservations.Add(reservation);

        var promotedPosition = head.Position;
        db.WaitingListEntries.Remove(head);

        var followers = await db.WaitingListEntries
            .Where(w => w.ClassSessionId == classSessionId && w.Position > promotedPosition)
            .ToListAsync(ct);
        foreach (var follower in followers)
        {
            follower.Position--;
        }

        await db.SaveChangesAsync(ct);

        await events.PublishAsync(
            new SlotOpenedEvent(classSessionId, head.MemberId, reservation.Id, session.StartUtc),
            ct);

        return true;
    }
}
