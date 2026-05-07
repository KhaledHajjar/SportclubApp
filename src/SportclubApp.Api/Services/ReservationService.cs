using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Common;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Api.Services.Policies;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;
using SportclubApp.Shared.Errors;

namespace SportclubApp.Api.Services;

public sealed class ReservationService(
    AppDbContext db,
    ISubscriptionLimitPolicyFactory policyFactory,
    IWaitingListPromotionService waitingListPromotion,
    TimeProvider time) : IReservationService
{
    public static readonly TimeSpan ReservationWindow = TimeSpan.FromDays(7);
    public static readonly TimeSpan CancellationLockout = TimeSpan.FromHours(1);

    public async Task<ReservationResult> ReserveAsync(Guid memberId, Guid classSessionId, CancellationToken ct)
    {
        var now = time.GetUtcNow();

        var session = await db.ClassSessions.SingleOrDefaultAsync(c => c.Id == classSessionId, ct);
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

        var subscription = await db.Subscriptions
            .Where(s => s.MemberId == memberId && s.StartUtc <= now && s.EndUtc > now)
            .OrderByDescending(s => s.EndUtc)
            .FirstOrDefaultAsync(ct);
        if (subscription is null)
        {
            return ReservationResult.Fail(ReservationErrorTypes.NoActiveSubscription, "No active subscription.");
        }

        var (weekStart, weekEnd) = IsoWeek.GetCurrentRange(now);
        var weeklyCount = await db.Reservations.CountAsync(
            r => r.MemberId == memberId
                 && r.Status == ReservationStatus.Active
                 && r.ClassSession.StartUtc >= weekStart
                 && r.ClassSession.StartUtc < weekEnd, ct);

        var policy = policyFactory.GetFor(subscription.Type);
        if (!policy.CanReserve(weeklyCount))
        {
            return ReservationResult.Fail(ReservationErrorTypes.WeeklyLimitReached, "Weekly visit limit reached for your subscription.");
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
        await db.SaveChangesAsync(ct);

        return ReservationResult.Ok(new ReservationDto(
            Id: reservation.Id,
            ClassSessionId: classSessionId,
            ClassStartUtc: session.StartUtc,
            CreatedUtc: reservation.CreatedUtc,
            Status: reservation.Status));
    }

    public async Task<ReservationResult> CancelAsync(Guid memberId, Guid reservationId, CancellationToken ct)
    {
        var now = time.GetUtcNow();

        var reservation = await db.Reservations
            .Include(r => r.ClassSession)
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

        if (reservation.ClassSession.StartUtc - now < CancellationLockout)
        {
            return ReservationResult.Fail(ReservationErrorTypes.CancelTooLate, "Cancellation is only allowed up to one hour before class start.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        await waitingListPromotion.TryPromoteHeadAsync(reservation.ClassSessionId, ct);

        return ReservationResult.Ok(new ReservationDto(
            Id: reservation.Id,
            ClassSessionId: reservation.ClassSessionId,
            ClassStartUtc: reservation.ClassSession.StartUtc,
            CreatedUtc: reservation.CreatedUtc,
            Status: reservation.Status));
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
                r.Status))
            .ToListAsync(ct);
    }
}
