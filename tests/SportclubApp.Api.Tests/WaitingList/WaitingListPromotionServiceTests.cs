using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using SportclubApp.Api.Common.Events;
using SportclubApp.Api.Entities;
using SportclubApp.Api.Services;
using SportclubApp.Api.Tests.Infrastructure;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Tests.WaitingList;

// Defends: "When a member cancels and a slot opens, the next person on the
// waitlist gets the spot automatically and is told their slot is open."
// Exercises the Observer pattern (IDomainEventDispatcher + SlotOpenedEvent).
public sealed class WaitingListPromotionServiceTests
{
    [Fact]
    public async Task Promotes_head_shifts_followers_and_publishes_slot_opened_event()
    {
        var now = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var time = new FakeTimeProvider(now);

        await using var db = new TestDb();
        var session = db.SeedClassSession(startUtc: now.AddDays(1), capacity: 2);
        var head = db.SeedMember(suffix: "1");
        var middle = db.SeedMember(suffix: "2");
        var tail = db.SeedMember(suffix: "3");

        db.Context.WaitingListEntries.AddRange(
            new WaitingListEntry { Id = Guid.NewGuid(), MemberId = head.Id, ClassSessionId = session.Id, Position = 1, CreatedUtc = now.AddMinutes(-30) },
            new WaitingListEntry { Id = Guid.NewGuid(), MemberId = middle.Id, ClassSessionId = session.Id, Position = 2, CreatedUtc = now.AddMinutes(-20) },
            new WaitingListEntry { Id = Guid.NewGuid(), MemberId = tail.Id, ClassSessionId = session.Id, Position = 3, CreatedUtc = now.AddMinutes(-10) });
        await db.Context.SaveChangesAsync();

        var events = Substitute.For<IDomainEventDispatcher>();
        var sut = new WaitingListPromotionService(db.Context, events, time);

        var promoted = await sut.TryPromoteHeadAsync(session.Id, CancellationToken.None);

        Assert.True(promoted);

        var remaining = await db.Context.WaitingListEntries
            .Where(w => w.ClassSessionId == session.Id)
            .OrderBy(w => w.Position)
            .ToListAsync();
        Assert.Equal(2, remaining.Count);
        Assert.Equal(middle.Id, remaining[0].MemberId);
        Assert.Equal(1, remaining[0].Position);
        Assert.Equal(tail.Id, remaining[1].MemberId);
        Assert.Equal(2, remaining[1].Position);

        var newReservation = await db.Context.Reservations.SingleAsync();
        Assert.Equal(head.Id, newReservation.MemberId);
        Assert.Equal(session.Id, newReservation.ClassSessionId);
        Assert.Equal(ReservationStatus.Active, newReservation.Status);

        await events.Received(1).PublishAsync(
            Arg.Is<SlotOpenedEvent>(e =>
                e.ClassSessionId == session.Id
                && e.PromotedMemberId == head.Id
                && e.PromotedReservationId == newReservation.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Promotion_with_empty_waitlist_is_a_noop_and_publishes_no_event()
    {
        var now = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var time = new FakeTimeProvider(now);

        await using var db = new TestDb();
        var session = db.SeedClassSession(startUtc: now.AddDays(1), capacity: 5);
        await db.Context.SaveChangesAsync();

        var events = Substitute.For<IDomainEventDispatcher>();
        var sut = new WaitingListPromotionService(db.Context, events, time);

        var promoted = await sut.TryPromoteHeadAsync(session.Id, CancellationToken.None);

        Assert.False(promoted);
        Assert.Empty(db.Context.Reservations);
        await events.DidNotReceive().PublishAsync(Arg.Any<SlotOpenedEvent>(), Arg.Any<CancellationToken>());
    }
}
