using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using SportclubApp.Api.Services;
using SportclubApp.Api.Services.Policies;
using SportclubApp.Api.Tests.Infrastructure;
using SportclubApp.Shared.Enums;
using SportclubApp.Shared.Errors;

namespace SportclubApp.Api.Tests.Reservations;

// Defends: "As a Standard member I cannot back out of a class less than an hour
// before it starts; as a Premium member I get the perk of cancelling up to
// 15 minutes before start." Drives the Strategy pattern in
// PlanCancellationPolicyFactory through the real ReservationService path.
public sealed class ReservationServiceCancelTests
{
    [Fact]
    public async Task Cancel_within_one_hour_for_standard_tier_is_rejected_as_too_late()
    {
        var now = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var time = new FakeTimeProvider(now);

        await using var db = new TestDb();
        var member = db.SeedMember();
        var standardPlan = db.SeedPlan(PlanTier.Standard);
        db.SeedActiveSubscription(member, standardPlan, now);
        var session = db.SeedClassSession(startUtc: now.AddMinutes(30));
        var reservation = db.SeedActiveReservation(member, session, now);
        await db.Context.SaveChangesAsync();

        var promotion = Substitute.For<IWaitingListPromotionService>();
        var sut = new ReservationService(
            db.Context,
            new PlanCancellationPolicyFactory(),
            promotion,
            time);

        var result = await sut.CancelAsync(member.Id, reservation.Id, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(ReservationErrorTypes.CancelTooLate, result.ErrorType);
        await promotion.DidNotReceive().TryPromoteHeadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancel_within_one_hour_for_premium_tier_is_allowed()
    {
        var now = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var time = new FakeTimeProvider(now);

        await using var db = new TestDb();
        var member = db.SeedMember();
        var premiumPlan = db.SeedPlan(PlanTier.Premium);
        db.SeedActiveSubscription(member, premiumPlan, now);
        var session = db.SeedClassSession(startUtc: now.AddMinutes(30));
        var reservation = db.SeedActiveReservation(member, session, now);
        await db.Context.SaveChangesAsync();

        var promotion = Substitute.For<IWaitingListPromotionService>();
        var sut = new ReservationService(
            db.Context,
            new PlanCancellationPolicyFactory(),
            promotion,
            time);

        var result = await sut.CancelAsync(member.Id, reservation.Id, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ReservationStatus.Cancelled, db.Context.Reservations.Single().Status);
        await promotion.Received(1).TryPromoteHeadAsync(session.Id, Arg.Any<CancellationToken>());
    }
}
