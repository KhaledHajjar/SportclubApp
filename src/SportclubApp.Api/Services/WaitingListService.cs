using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;
using SportclubApp.Shared.Errors;

namespace SportclubApp.Api.Services;

public sealed class WaitingListService(AppDbContext db, TimeProvider time) : IWaitingListService
{
    public async Task<WaitingListResult> JoinAsync(Guid memberId, Guid classSessionId, CancellationToken ct)
    {
        var now = time.GetUtcNow();

        var session = await db.ClassSessions.SingleOrDefaultAsync(c => c.Id == classSessionId, ct);
        if (session is null)
        {
            return WaitingListResult.Fail(ReservationErrorTypes.ClassNotFound, "Class not found.");
        }

        if (session.StartUtc <= now)
        {
            return WaitingListResult.Fail(ReservationErrorTypes.ClassAlreadyStarted, "Class has already started.");
        }

        var reservedCount = await db.Reservations.CountAsync(
            r => r.ClassSessionId == classSessionId && r.Status == ReservationStatus.Active, ct);
        if (reservedCount < session.Capacity)
        {
            return WaitingListResult.Fail(WaitingListErrorTypes.ClassNotFull, "Class still has free spots; reserve directly instead.");
        }

        var alreadyReserved = await db.Reservations.AnyAsync(
            r => r.MemberId == memberId && r.ClassSessionId == classSessionId && r.Status == ReservationStatus.Active, ct);
        if (alreadyReserved)
        {
            return WaitingListResult.Fail(ReservationErrorTypes.AlreadyReserved, "You already have a reservation for this class.");
        }

        var alreadyOnList = await db.WaitingListEntries.AnyAsync(
            w => w.MemberId == memberId && w.ClassSessionId == classSessionId, ct);
        if (alreadyOnList)
        {
            return WaitingListResult.Fail(WaitingListErrorTypes.AlreadyOnWaitingList, "You are already on the waiting list.");
        }

        var nextPosition = await db.WaitingListEntries.CountAsync(w => w.ClassSessionId == classSessionId, ct) + 1;

        var entry = new WaitingListEntry
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            ClassSessionId = classSessionId,
            Position = nextPosition,
            CreatedUtc = now,
        };
        db.WaitingListEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        return WaitingListResult.Ok(new WaitingListEntryDto(entry.Id, classSessionId, entry.Position, entry.CreatedUtc));
    }

    public async Task<WaitingListResult> LeaveAsync(Guid memberId, Guid entryId, CancellationToken ct)
    {
        var entry = await db.WaitingListEntries.SingleOrDefaultAsync(w => w.Id == entryId, ct);
        if (entry is null)
        {
            return WaitingListResult.Fail(WaitingListErrorTypes.WaitingListEntryNotFound, "Waiting list entry not found.");
        }

        if (entry.MemberId != memberId)
        {
            return WaitingListResult.Fail(WaitingListErrorTypes.WaitingListEntryNotOwned, "Waiting list entry does not belong to the current member.");
        }

        var classSessionId = entry.ClassSessionId;
        var leavingPosition = entry.Position;

        db.WaitingListEntries.Remove(entry);

        var followers = await db.WaitingListEntries
            .Where(w => w.ClassSessionId == classSessionId && w.Position > leavingPosition)
            .ToListAsync(ct);
        foreach (var follower in followers)
        {
            follower.Position--;
        }

        await db.SaveChangesAsync(ct);

        return WaitingListResult.Ok(new WaitingListEntryDto(entry.Id, classSessionId, leavingPosition, entry.CreatedUtc));
    }

    public async Task<IReadOnlyList<WaitingListEntryDto>> GetMineAsync(Guid memberId, CancellationToken ct)
    {
        return await db.WaitingListEntries
            .AsNoTracking()
            .Where(w => w.MemberId == memberId)
            .OrderBy(w => w.ClassSession.StartUtc)
            .Select(w => new WaitingListEntryDto(w.Id, w.ClassSessionId, w.Position, w.CreatedUtc))
            .ToListAsync(ct);
    }
}
