using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        // ---- Plan catalog ----
        var standardMonthly = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Standard Monthly",
            Tier = PlanTier.Standard,
            BillingPeriod = BillingPeriod.Monthly,
            DurationDays = 30,
            Price = 29.95m,
        };
        var standardYearly = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Standard Yearly",
            Tier = PlanTier.Standard,
            BillingPeriod = BillingPeriod.Yearly,
            DurationDays = 365,
            Price = 299.00m,
        };
        var premiumMonthly = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Premium Monthly",
            Tier = PlanTier.Premium,
            BillingPeriod = BillingPeriod.Monthly,
            DurationDays = 30,
            Price = 44.95m,
        };
        var premiumYearly = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Premium Yearly",
            Tier = PlanTier.Premium,
            BillingPeriod = BillingPeriod.Yearly,
            DurationDays = 365,
            Price = 449.00m,
        };
        var plans = new[] { standardMonthly, standardYearly, premiumMonthly, premiumYearly };
        await db.Plans.AddRangeAsync(plans, ct);

        // ---- Workouts ----
        var yoga = new Workout
        {
            Id = Guid.NewGuid(),
            Name = "Yoga",
            Description = "Vinyasa flow linking breath with movement. Suitable for all levels — modifications offered throughout. Bring your own mat or borrow one at reception.",
            DurationMinutes = 60,
        };
        var hiit = new Workout
        {
            Id = Guid.NewGuid(),
            Name = "HIIT",
            Description = "High-intensity interval training: thirty-second work sets with fifteen-second rests across functional and cardio stations. Intermediate to advanced; wear supportive shoes.",
            DurationMinutes = 45,
        };
        var spinning = new Workout
        {
            Id = Guid.NewGuid(),
            Name = "Spinning",
            Description = "Indoor cycling with themed playlists, hill profiles and sprint sets. The resistance dial is yours — all fitness levels welcome. Cycling shoes optional, clip-in pedals provided.",
            DurationMinutes = 50,
        };
        var pilates = new Workout
        {
            Id = Guid.NewGuid(),
            Name = "Pilates",
            Description = "Mat-based Pilates focused on core stability, posture and controlled mobility. Suitable for all levels; participants with chronic injuries please check in with the instructor first.",
            DurationMinutes = 55,
        };
        var workouts = new[] { yoga, hiit, spinning, pilates };
        await db.Workouts.AddRangeAsync(workouts, ct);

        // ---- Locations ----
        var deLoft = new Location
        {
            Id = Guid.NewGuid(),
            Name = "De Loft",
            Address = "Top floor, north wing — wooden floor, mirror wall, props rack.",
        };
        var cycleStudio = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Cycle Studio",
            Address = "Ground floor, west wing — twenty stationary bikes with surround sound.",
        };
        var sportzaal = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Sportzaal",
            Address = "Ground floor, east wing — open functional-training hall with rowers, kettlebells and a sprint track.",
        };
        var locations = new[] { deLoft, cycleStudio, sportzaal };
        await db.Locations.AddRangeAsync(locations, ct);

        // ---- Users (Diana first so the Instructor entity can reference her MemberId) ----
        var diana = await CreateUserAsync(
            "diana@sportclub.test", "Diana", "Smit", new DateOnly(1985, 4, 15), AuthRoles.Instructor, ct);
        var admin = await CreateUserAsync(
            "admin@sportclub.test", "Sportclub", "Admin", new DateOnly(1980, 1, 1), AuthRoles.Admin, ct);
        var alice = await CreateUserAsync(
            "alice@sportclub.test", "Alice", "de Vries", new DateOnly(1995, 8, 22), AuthRoles.Member, ct);
        var bob = await CreateUserAsync(
            "bob@sportclub.test", "Bob", "Jansen", new DateOnly(1988, 2, 10), AuthRoles.Member, ct);
        var charlie = await CreateUserAsync(
            "charlie@sportclub.test", "Charlie", "Bakker", new DateOnly(1992, 11, 30), AuthRoles.Member, ct);
        var test = await CreateUserAsync(
            "test@test.com", "Test", "User", new DateOnly(2000, 1, 15), AuthRoles.Member, ct);

        // ---- Instructors ----
        var dianaInstructor = new Instructor
        {
            Id = Guid.NewGuid(),
            FirstName = "Diana",
            LastName = "Smit",
            Bio = "Yoga Alliance E-RYT500 since 2014. Diana studied vinyasa in Mysore and teaches a strong, breath-led flow tailored to all bodies. She also leads the gym's mat Pilates programme.",
            MemberId = diana.Id,
        };
        var marcoInstructor = new Instructor
        {
            Id = Guid.NewGuid(),
            FirstName = "Marco",
            LastName = "Vermeer",
            Bio = "Former Dutch track cyclist turned spin coach. Marco's hour-long rides mix themed playlists with interval and hill profiles — leave your watt meter at the door and just chase the beat.",
        };
        var evaInstructor = new Instructor
        {
            Id = Guid.NewGuid(),
            FirstName = "Eva",
            LastName = "Hendriks",
            Bio = "ACE-certified personal trainer and HIIT specialist. Eva's circuits push you hard but keep technique honest; modifications are always on offer for first-timers and seasoned athletes alike.",
        };
        var instructors = new[] { dianaInstructor, marcoInstructor, evaInstructor };
        await db.Instructors.AddRangeAsync(instructors, ct);

        // ---- Workout → instructor + location mapping (each workout is consistently taught and located) ----
        var workoutInstructor = new Dictionary<Guid, Guid>
        {
            [yoga.Id] = dianaInstructor.Id,
            [pilates.Id] = dianaInstructor.Id,
            [spinning.Id] = marcoInstructor.Id,
            [hiit.Id] = evaInstructor.Id,
        };
        var workoutLocation = new Dictionary<Guid, Guid>
        {
            [yoga.Id] = deLoft.Id,
            [pilates.Id] = deLoft.Id,
            [spinning.Id] = cycleStudio.Id,
            [hiit.Id] = sportzaal.Id,
        };

        // ---- Subscriptions ----
        // alice    Standard Monthly  ends in 10 days   common case
        // bob      Standard Yearly   ends in 40 days   inside 6-week expiry warning window (US-10)
        // charlie  Premium Monthly   ends in 15 days   demo target for tier-based cancellation lockout
        // test     Standard Monthly  ends in 20 days   quick-login convenience
        db.Subscriptions.AddRange(
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = alice.Id,
                PlanId = standardMonthly.Id,
                StartUtc = now.AddDays(-20),
                EndUtc = now.AddDays(10),
            },
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = bob.Id,
                PlanId = standardYearly.Id,
                StartUtc = now.AddDays(-325),
                EndUtc = now.AddDays(40),
            },
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = charlie.Id,
                PlanId = premiumMonthly.Id,
                StartUtc = now.AddDays(-15),
                EndUtc = now.AddDays(15),
            },
            new Subscription
            {
                Id = Guid.NewGuid(),
                MemberId = test.Id,
                PlanId = standardMonthly.Id,
                StartUtc = now.AddDays(-10),
                EndUtc = now.AddDays(20),
            });

        // ---- Class sessions: 3 slots/day at 09:00, 12:30, 19:00 over 56 days ----
        // 09:00 mind-body block, 12:30 interval block, 19:00 mixed evening block.
        var morningRotation = new[] { yoga, pilates };
        var lunchRotation = new[] { hiit, spinning };
        var eveningRotation = new[] { yoga, spinning, pilates, hiit };

        var sessions = new List<ClassSession>();
        var startOfDay = new DateTimeOffset(DateOnly.FromDateTime(now.UtcDateTime).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        for (var d = -42; d < 14; d++)
        {
            var day = startOfDay.AddDays(d);
            var dayKey = d + 100; // keep the modulo positive even for negative offsets
            var slots = new (double Hours, Workout Workout)[]
            {
                (9.0, morningRotation[dayKey % morningRotation.Length]),
                (12.5, lunchRotation[dayKey % lunchRotation.Length]),
                (19.0, eveningRotation[dayKey % eveningRotation.Length]),
            };

            foreach (var (hours, workout) in slots)
            {
                sessions.Add(new ClassSession
                {
                    Id = Guid.NewGuid(),
                    StartUtc = day.AddHours(hours),
                    Capacity = 10,
                    WorkoutId = workout.Id,
                    InstructorId = workoutInstructor[workout.Id],
                    LocationId = workoutLocation[workout.Id],
                });
            }
        }

        // ---- Demo class: first future Tuesday 09:00 → Yoga at capacity 2 ----
        // The 09:00 slot is always Yoga or Pilates by the rotation above, but force Yoga + cap 2
        // here so the waitlist + slot-opened demo flow stays deterministic.
        var demoClass = sessions
            .Where(s => s.StartUtc > now.AddHours(12)
                        && s.StartUtc.UtcDateTime.DayOfWeek == DayOfWeek.Tuesday
                        && s.StartUtc.UtcDateTime.Hour == 9)
            .OrderBy(s => s.StartUtc)
            .First();
        demoClass.WorkoutId = yoga.Id;
        demoClass.InstructorId = dianaInstructor.Id;
        demoClass.LocationId = deLoft.Id;
        demoClass.Capacity = 2;

        await db.ClassSessions.AddRangeAsync(sessions, ct);

        // ---- Member workout preferences (drives reservations + attendance) ----
        var alicePrefs = new[] { yoga.Id, pilates.Id };                    // mind-body regular
        var bobPrefs = new[] { yoga.Id, spinning.Id, hiit.Id, pilates.Id }; // generalist, anything goes
        var charliePrefs = new[] { hiit.Id, spinning.Id };                  // high-intensity
        var testPrefs = new[] { yoga.Id };                                  // occasional yoga drop-in

        // ---- Reservations ----
        var futurePool = sessions
            .Where(s => s.StartUtc > now.AddHours(2) && s.Id != demoClass.Id)
            .OrderBy(s => s.StartUtc)
            .ToList();

        ClassSession PickPreferred(ICollection<Guid> preferredWorkoutIds, int offset)
        {
            var filtered = futurePool
                .Where(s => preferredWorkoutIds.Contains(s.WorkoutId))
                .ToList();
            return filtered.Count > 0
                ? filtered[offset % filtered.Count]
                : futurePool[offset % futurePool.Count];
        }

        db.Reservations.AddRange(
            // Demo class: alice + bob pre-booked (capacity 2 → cancelling alice triggers waitlist promotion for charlie)
            ReservationFor(alice, demoClass, now.AddDays(-2)),
            ReservationFor(bob, demoClass, now.AddDays(-2)),
            // Extras matched to each member's preferences
            ReservationFor(alice, PickPreferred(alicePrefs, 1), now.AddDays(-1)),
            ReservationFor(bob, PickPreferred(bobPrefs, 5), now.AddDays(-1)),
            ReservationFor(charlie, PickPreferred(charliePrefs, 2), now.AddDays(-1)),
            ReservationFor(charlie, PickPreferred(charliePrefs, 6), now.AddDays(-1)),
            ReservationFor(test, PickPreferred(testPrefs, 3), now.AddDays(-1)),
            ReservationFor(test, PickPreferred(testPrefs, 7), now.AddDays(-1)));

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

        // ---- Attendance: 5 rows per non-instructor, drawn from preferred workouts ----
        var pastSessions = sessions
            .Where(s => s.StartUtc < now)
            .OrderBy(s => s.StartUtc)
            .ToList();

        SeedAttendance(alice, pastSessions, workouts, alicePrefs, offset: 0);
        SeedAttendance(bob, pastSessions, workouts, bobPrefs, offset: 3);
        SeedAttendance(charlie, pastSessions, workouts, charliePrefs, offset: 6);
        SeedAttendance(test, pastSessions, workouts, testPrefs, offset: 9);

        // ---- One already-read SlotOpened notification per non-instructor, so the tab isn't empty ----
        SeedReadNotification(alice, pastSessions, now, hashSeed: 0);
        SeedReadNotification(bob, pastSessions, now, hashSeed: 1);
        SeedReadNotification(charlie, pastSessions, now, hashSeed: 2);
        SeedReadNotification(test, pastSessions, now, hashSeed: 3);

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded {Plans} plans, {Workouts} workouts, {Locations} locations, {Instructors} instructors, {Members} members, {Sessions} sessions. Demo class {DemoClassId} at {DemoClassStart:u}.",
            plans.Length, workouts.Length, locations.Length, instructors.Length, 6, sessions.Count, demoClass.Id, demoClass.StartUtc);
    }

    private static Reservation ReservationFor(Member member, ClassSession session, DateTimeOffset createdUtc) => new()
    {
        Id = Guid.NewGuid(),
        MemberId = member.Id,
        ClassSessionId = session.Id,
        CreatedUtc = createdUtc,
        Status = ReservationStatus.Active,
    };

    private void SeedAttendance(
        Member member,
        IReadOnlyList<ClassSession> pastSessions,
        Workout[] workouts,
        ICollection<Guid> preferredWorkoutIds,
        int offset)
    {
        var preferredPast = pastSessions
            .Where(s => preferredWorkoutIds.Contains(s.WorkoutId))
            .ToList();

        var pool = preferredPast.Count >= 5 ? preferredPast : pastSessions;
        if (pool.Count == 0)
        {
            return;
        }

        var step = Math.Max(1, pool.Count / 5);
        for (var i = 0; i < 5; i++)
        {
            var index = (offset + i * step) % pool.Count;
            var session = pool[index];
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

    private async Task<Member> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string role,
        CancellationToken ct)
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
            DateOfBirth = dateOfBirth,
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
