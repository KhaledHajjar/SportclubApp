using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Tests.Infrastructure;

// In-memory SQLite database with the real schema (EnsureCreated builds from the
// model, including the unique-filtered active-reservation index). One connection
// per fixture — SQLite gives each connection its own private :memory: database,
// so the connection must outlive the context.
internal sealed class TestDb : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public Member SeedMember(string suffix = "")
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Member" + suffix,
            UserName = $"member{suffix}@test.local",
            NormalizedUserName = $"MEMBER{suffix}@TEST.LOCAL",
            Email = $"member{suffix}@test.local",
            NormalizedEmail = $"MEMBER{suffix}@TEST.LOCAL",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        Context.Set<Member>().Add(member);
        return member;
    }

    public Plan SeedPlan(PlanTier tier)
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = tier == PlanTier.Premium ? "Premium Monthly" : "Standard Monthly",
            Tier = tier,
            BillingPeriod = BillingPeriod.Monthly,
            DurationDays = 30,
            Price = tier == PlanTier.Premium ? 49.99m : 29.99m,
        };
        Context.Plans.Add(plan);
        return plan;
    }

    public Subscription SeedActiveSubscription(Member member, Plan plan, DateTimeOffset now)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            PlanId = plan.Id,
            StartUtc = now.AddDays(-1),
            EndUtc = now.AddDays(plan.DurationDays),
        };
        Context.Subscriptions.Add(subscription);
        return subscription;
    }

    public ClassSession SeedClassSession(DateTimeOffset startUtc, int capacity = 10)
    {
        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            Name = "Test Workout",
            Description = "A workout for tests.",
            DurationMinutes = 60,
        };
        var location = new Location { Id = Guid.NewGuid(), Name = "Test Studio" };
        var instructor = new Instructor
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Instructor",
        };
        var session = new ClassSession
        {
            Id = Guid.NewGuid(),
            StartUtc = startUtc,
            Capacity = capacity,
            WorkoutId = workout.Id,
            Workout = workout,
            InstructorId = instructor.Id,
            Instructor = instructor,
            LocationId = location.Id,
            Location = location,
        };
        Context.Workouts.Add(workout);
        Context.Locations.Add(location);
        Context.Instructors.Add(instructor);
        Context.ClassSessions.Add(session);
        return session;
    }

    public Reservation SeedActiveReservation(Member member, ClassSession session, DateTimeOffset now)
    {
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            ClassSessionId = session.Id,
            CreatedUtc = now.AddHours(-2),
            Status = ReservationStatus.Active,
        };
        Context.Reservations.Add(reservation);
        return reservation;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
