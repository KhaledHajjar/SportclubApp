using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Common;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Enums;
using SportclubApp.Shared.Notifications;

namespace SportclubApp.Api.Data;

public sealed class DbSeeder(
    AppDbContext db,
    UserManager<Member> userManager,
    ILogger<DbSeeder> logger)
{
    private const string DemoPassword = "Test1234!";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Workouts.AnyAsync(ct))
        {
            logger.LogInformation("Database already contains seed data; skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        var now = DateTimeOffset.UtcNow;

        // ---- Workouts and locations ----
        var workouts = new[]
        {
            new Workout { Id = Guid.NewGuid(), Name = "Yoga", Description = "Vinyasa flow yoga for all levels.", DurationMinutes = 60 },
            new Workout { Id = Guid.NewGuid(), Name = "HIIT", Description = "High-intensity interval training.", DurationMinutes = 45 },
            new Workout { Id = Guid.NewGuid(), Name = "Spinning", Description = "Indoor cycling with music.", DurationMinutes = 50 },
            new Workout { Id = Guid.NewGuid(), Name = "Pilates", Description = "Core strength and flexibility.", DurationMinutes = 55 },
        };
        var locations = new[]
        {
            new Location { Id = Guid.NewGuid(), Name = "Studio 1", Address = "Main gym, ground floor" },
            new Location { Id = Guid.NewGuid(), Name = "Studio 2", Address = "Spinning room, first floor" },
        };
        await db.Workouts.AddRangeAsync(workouts, ct);
        await db.Locations.AddRangeAsync(locations, ct);

        var yoga = workouts.First(w => w.Name == "Yoga");

        // ---- Users (Diana first so the Instructor entity can reference her MemberId) ----
        var diana = await CreateUserAsync("diana@sportclub.test", "Diana", "Smit", AuthRoles.Instructor, ct);
        var alice = await CreateUserAsync("alice@sportclub.test", "Alice", "de Vries", AuthRoles.Member, ct);
        var bob = await CreateUserAsync("bob@sportclub.test", "Bob", "Jansen", AuthRoles.Member, ct);
        var charlie = await CreateUserAsync("charlie@sportclub.test", "Charlie", "Bakker", AuthRoles.Member, ct);
        var test = await CreateUserAsync("test@test.com", "Test", "User", AuthRoles.Member, ct);

        // ---- Instructors ----
        var instructors = new[]
        {
            new Instructor { Id = Guid.NewGuid(), FirstName = "Diana", LastName = "Smit", Bio = "Senior coach.", MemberId = diana.Id },
            new Instructor { Id = Guid.NewGuid(), FirstName = "John", LastName = "Smith", Bio = "Cycling specialist." },
            new Instructor { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Bio = "Yoga and Pilates teacher." },
        };
        await db.Instructors.AddRangeAsync(instructors, ct);

        // ---- Subscriptions ----
        db.Subscriptions.AddRange(
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = alice.Id,
                Type = SubscriptionType.TwicePerWeek,
                StartUtc = now.AddDays(-30),
                EndUtc = now.AddMonths(6),
            },
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = bob.Id,
                Type = SubscriptionType.Yearly,
                StartUtc = now.AddDays(-30),
                // 40 days remaining → triggers the 6-week subscription-expiry local notification on login
                EndUtc = now.AddDays(40),
            },
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = charlie.Id,
                Type = SubscriptionType.Unlimited,
                StartUtc = now.AddDays(-30),
                EndUtc = now.AddYears(1),
            },
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = test.Id,
                Type = SubscriptionType.TwicePerWeek,
                StartUtc = now.AddDays(-30),
                EndUtc = now.AddMonths(6),
            });

        // ---- Class sessions: 42 days past + 14 days future, 9:00 and 18:00 ----
        var sessions = new List<ClassSession>();
        var startOfDay = new DateTimeOffset(DateOnly.FromDateTime(now.UtcDateTime).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var rotation = 0;
        for (var d = -42; d < 14; d++)
        {
            var day = startOfDay.AddDays(d);
            foreach (var hour in new[] { 9, 18 })
            {
                sessions.Add(new ClassSession
                {
                    Id = Guid.NewGuid(),
                    StartUtc = day.AddHours(hour),
                    Capacity = 10,
                    WorkoutId = workouts[rotation % workouts.Length].Id,
                    InstructorId = instructors[rotation % instructors.Length].Id,
                    LocationId = locations[rotation % locations.Length].Id,
                });
                rotation++;
            }
        }

        // ---- Waitlist demo class: first future Tuesday 09:00 with at least 12h headroom ----
        var demoClass = sessions
            .Where(s => s.StartUtc > now.AddHours(12)
                        && s.StartUtc.UtcDateTime.DayOfWeek == DayOfWeek.Tuesday
                        && s.StartUtc.UtcDateTime.Hour == 9)
            .OrderBy(s => s.StartUtc)
            .First();
        demoClass.WorkoutId = yoga.Id;
        demoClass.Capacity = 2;

        await db.ClassSessions.AddRangeAsync(sessions, ct);

        // ---- Reservations ----
        var futurePool = sessions
            .Where(s => s.StartUtc > now.AddHours(2) && s.Id != demoClass.Id)
            .OrderBy(s => s.StartUtc)
            .ToList();

        var (demoWeekStart, demoWeekEnd) = IsoWeek.GetCurrentRange(demoClass.StartUtc);

        // Alice (TwicePerWeek): exactly 2 in the demo class's week
        var aliceSecond = futurePool.First(s =>
            s.StartUtc >= demoWeekStart && s.StartUtc < demoWeekEnd);

        // Bob (Yearly): demo class + 1 in a later week
        var bobSecond = futurePool.First(s => s.StartUtc >= demoWeekEnd);

        // Charlie (Unlimited) and Test (TwicePerWeek): 2 each, different weeks, none being the demo class
        var charlieFirst = futurePool.First(s =>
            (s.StartUtc - now).TotalDays >= 3 && (s.StartUtc - now).TotalDays < 6);
        var charlieSecond = futurePool.First(s =>
            (s.StartUtc - now).TotalDays >= 9 && (s.StartUtc - now).TotalDays < 13
            && s.Id != charlieFirst.Id);

        var testFirst = futurePool.First(s =>
            (s.StartUtc - now).TotalDays >= 4 && (s.StartUtc - now).TotalDays < 7
            && s.Id != charlieFirst.Id);
        var testSecond = futurePool.First(s =>
            (s.StartUtc - now).TotalDays >= 10 && (s.StartUtc - now).TotalDays < 14
            && s.Id != charlieSecond.Id);

        db.Reservations.AddRange(
            ReservationFor(alice, demoClass, now.AddDays(-2)),
            ReservationFor(alice, aliceSecond, now.AddDays(-1)),
            ReservationFor(bob, demoClass, now.AddDays(-2)),
            ReservationFor(bob, bobSecond, now.AddDays(-1)),
            ReservationFor(charlie, charlieFirst, now.AddDays(-1)),
            ReservationFor(charlie, charlieSecond, now.AddDays(-1)),
            ReservationFor(test, testFirst, now.AddDays(-1)),
            ReservationFor(test, testSecond, now.AddDays(-1)));

        // ---- Waitlist on the demo class: Charlie head, Test second ----
        db.WaitingListEntries.AddRange(
            new WaitingListEntry
            {
                Id = Guid.NewGuid(),
                MemberId = charlie.Id,
                ClassSessionId = demoClass.Id,
                Position = 1,
                CreatedUtc = now.AddDays(-1),
            },
            new WaitingListEntry
            {
                Id = Guid.NewGuid(),
                MemberId = test.Id,
                ClassSessionId = demoClass.Id,
                Position = 2,
                CreatedUtc = now.AddHours(-12),
            });

        // ---- Attendance: 5 rows per non-instructor, spread across the past 6 weeks ----
        var pastSessions = sessions
            .Where(s => s.StartUtc < now)
            .OrderBy(s => s.StartUtc)
            .ToList();

        SeedAttendance(alice, pastSessions, workouts, offset: 0);
        SeedAttendance(bob, pastSessions, workouts, offset: 3);
        SeedAttendance(charlie, pastSessions, workouts, offset: 6);
        SeedAttendance(test, pastSessions, workouts, offset: 9);

        // ---- One already-read SlotOpened notification per non-instructor, so the tab isn't empty ----
        SeedReadNotification(alice, pastSessions, now, hashSeed: 0);
        SeedReadNotification(bob, pastSessions, now, hashSeed: 1);
        SeedReadNotification(charlie, pastSessions, now, hashSeed: 2);
        SeedReadNotification(test, pastSessions, now, hashSeed: 3);

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded {Workouts} workouts, {Locations} locations, {Instructors} instructors, {Members} members, {Sessions} sessions. Demo class {DemoClassId} at {DemoClassStart:u}.",
            workouts.Length, locations.Length, instructors.Length, 5, sessions.Count, demoClass.Id, demoClass.StartUtc);
    }

    private static Reservation ReservationFor(Member member, ClassSession session, DateTimeOffset createdUtc) => new()
    {
        Id = Guid.NewGuid(),
        MemberId = member.Id,
        ClassSessionId = session.Id,
        CreatedUtc = createdUtc,
        Status = ReservationStatus.Active,
    };

    private void SeedAttendance(Member member, IReadOnlyList<ClassSession> pastSessions, Workout[] workouts, int offset)
    {
        if (pastSessions.Count == 0)
        {
            return;
        }

        var step = Math.Max(1, pastSessions.Count / 5);
        for (var i = 0; i < 5; i++)
        {
            var index = (offset + i * step) % pastSessions.Count;
            var session = pastSessions[index];
            var workout = workouts.First(w => w.Id == session.WorkoutId);
            db.AttendanceRecords.Add(new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                ClassSessionId = session.Id,
                AttendedUtc = session.StartUtc.AddMinutes(workout.DurationMinutes),
            });
        }
    }

    private void SeedReadNotification(Member member, IReadOnlyList<ClassSession> pastSessions, DateTimeOffset now, int hashSeed)
    {
        if (pastSessions.Count == 0)
        {
            return;
        }

        var pastClass = pastSessions[(hashSeed * 7) % pastSessions.Count];
        var data = new Dictionary<string, string>
        {
            [NotificationDataKeys.ClassSessionId] = pastClass.Id.ToString(),
        };
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Type = NotificationTypes.SlotOpened,
            Title = "A spot opened earlier this week",
            Body = "You moved off the waiting list and got a confirmed spot.",
            DataJson = JsonSerializer.Serialize(data),
            CreatedUtc = now.AddDays(-3),
            ReadUtc = now.AddDays(-2),
        });
    }

    private async Task<Member> CreateUserAsync(string email, string firstName, string lastName, string role, CancellationToken ct)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new Member
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            CreatedUtc = DateTimeOffset.UtcNow,
        };

        var result = await userManager.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to seed user {email}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
