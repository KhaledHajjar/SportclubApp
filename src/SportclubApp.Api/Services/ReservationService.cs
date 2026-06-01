using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Api.Services.Policies;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;
using SportclubApp.Shared.Errors;

namespace SportclubApp.Api.Services;

public sealed class ReservationService(
    AppDbContext db,
    IPlanCancellationPolicyFactory cancellationPolicies,
    IWaitingListPromotionService waitingListPromotion,
    TimeProvider time) : IReservationService
{
    public static readonly TimeSpan ReservationWindow = TimeSpan.FromDays(7);

    public async Task<ReservationResult> ReserveAsync(Guid memberId, Guid classSessionId, CancellationToken ct)
    {
        // Duplicate active-reservation races are caught by a unique filtered index
        // (see AppDbContext). Capacity-overrun under concurrent writes still needs
        // a RowVersion on ClassSession for an atomic compare-and-swap — see README
        // "Limitations".
        var now = time.GetUtcNow();

        var session = await db.ClassSessions
            .Include(c => c.Workout)
            .Include(c => c.Location)
            .SingleOrDefaultAsync(c => c.Id == classSessionId, ct);
        if (session is null)
        {
            return ReservationResult.Fail(ReservationErrorTypes.ClassNotFound, "Class not found.");
        }

        if (session.StartUtc <= now)
        {
            return ReservationResult.Fail(ReservationErrorTypes.ClassAlreadyStarted, "Class has already started.");
        }

        if (session.StartUtc - now > ReservationWindow)
        {
            return ReservationResult.Fail(ReservationErrorTypes.ClassTooFarAhead, "Reservations open at most one week ahead.");
        }

        var alreadyReserved = await db.Reservations.AnyAsync(
            r => r.MemberId == memberId && r.ClassSessionId == classSessionId && r.Status == ReservationStatus.Active, ct);
        if (alreadyReserved)
        {
            return ReservationResult.Fail(ReservationErrorTypes.AlreadyReserved, "You already have a reservation for this class.");
        }

        var reservedCount = await db.Reservations.CountAsync(
            r => r.ClassSessionId == classSessionId && r.Status == ReservationStatus.Active, ct);
        if (reservedCount >= session.Capacity)
        {
            return ReservationResult.Fail(ReservationErrorTypes.ClassFull, "Class is full. Join the waiting list instead.");
        }

        var hasActiveSubscription = await db.Subscriptions.AnyAsync(
            s => s.MemberId == memberId && s.StartUtc <= now && s.EndUtc > now, ct);
        if (!hasActiveSubscription)
        {
            return ReservationResult.Fail(ReservationErrorTypes.NoActiveSubscription, "No active subscription.");
        }

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            ClassSessionId = classSessionId,
            CreatedUtc = now,
            Status = ReservationStatus.Active,
        };
        db.Reservations.Add(reservation);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Concurrent request created an active reservation for the same
            // (member, class) pair — caught by the unique filtered index.
            return ReservationResult.Fail(
                ReservationErrorTypes.AlreadyReserved,
                "You already have a reservation for this class.");
        }

        return ReservationResult.Ok(new ReservationDto(
            Id: reservation.Id,
            ClassSessionId: classSessionId,
            ClassStartUtc: session.StartUtc,
            CreatedUtc: reservation.CreatedUtc,
            Status: reservation.Status,
            WorkoutName: session.Workout.Name,
            LocationName: session.Location.Name));
    }

    public async Task<ReservationResult> CancelAsync(Guid memberId, Guid reservationId, CancellationToken ct)
    {
        var now = time.GetUtcNow();

        var reservation = await db.Reservations
            .Include(r => r.ClassSession).ThenInclude(c => c.Workout)
            .Include(r => r.ClassSession).ThenInclude(c => c.Location)
            .SingleOrDefaultAsync(r => r.Id == reservationId, ct);

        if (reservation is null)
        {
            return ReservationResult.Fail(ReservationErrorTypes.ReservationNotFound, "Reservation not found.");
        }

        if (reservation.MemberId != memberId)
        {
            return ReservationResult.Fail(ReservationErrorTypes.ReservationNotOwned, "Reservation does not belong to the current member.");
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            return ReservationResult.Fail(ReservationErrorTypes.ReservationNotFound, "Reservation is no longer active.");
        }

        // Cancellation lockout is plan-tier-driven (Strategy):
        // Standard members get 1h, Premium get 15min. If the member has no
        // active subscription at the moment of cancel, fall back to Standard.
        var activeTier = await db.Subscriptions
            .Where(s => s.MemberId == memberId && s.StartUtc <= now && s.EndUtc > now)
            .OrderByDescending(s => s.EndUtc)
            .Select(s => (PlanTier?)s.Plan.Tier)
            .FirstOrDefaultAsync(ct);

        var lockout = cancellationPolicies
            .GetFor(activeTier ?? PlanTier.Standard)
            .CancellationLockout;

        if (reservation.ClassSession.StartUtc - now < lockout)
        {
            return ReservationResult.Fail(
                ReservationErrorTypes.CancelTooLate,
                $"Cancellation is only allowed up to {FormatLockout(lockout)} before class start.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        await waitingListPromotion.TryPromoteHeadAsync(reservation.ClassSessionId, ct);

        return ReservationResult.Ok(new ReservationDto(
            Id: reservation.Id,
            ClassSessionId: reservation.ClassSessionId,
            ClassStartUtc: reservation.ClassSession.StartUtc,
            CreatedUtc: reservation.CreatedUtc,
            Status: reservation.Status,
            WorkoutName: reservation.ClassSession.Workout.Name,
            LocationName: reservation.ClassSession.Location.Name));
    }

    public async Task<IReadOnlyList<ReservationDto>> GetMineAsync(Guid memberId, CancellationToken ct)
    {
        return await db.Reservations
            .AsNoTracking()
            .Where(r => r.MemberId == memberId)
            .OrderBy(r => r.ClassSession.StartUtc)
            .Select(r => new ReservationDto(
                r.Id,
                r.ClassSessionId,
                r.ClassSession.StartUtc,
                r.CreatedUtc,
                r.Status,
                r.ClassSession.Workout.Name,
                r.ClassSession.Location.Name))
            .ToListAsync(ct);
    }

    private static string FormatLockout(TimeSpan lockout) =>
        lockout.TotalHours >= 1
            ? $"{lockout.TotalHours:0} hour{(lockout.TotalHours >= 2 ? "s" : string.Empty)}"
            : $"{lockout.TotalMinutes:0} minutes";
}
