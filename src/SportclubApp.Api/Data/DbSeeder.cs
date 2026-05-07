using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Data;

public sealed class DbSeeder(
    AppDbContext db,
    UserManager<Member> userManager,
    ILogger<DbSeeder> logger)
{
    private const string DemoPassword = "Password123!";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (await db.Workouts.AnyAsync(ct))
        {
            logger.LogInformation("Database already contains seed data; skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

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

        var instructorUser = await CreateUserAsync(
            email: "instructor@sportclub.test",
            firstName: "Iris",
            lastName: "Instructor",
            role: AuthRoles.Instructor,
            ct: ct);

        var instructors = new[]
        {
            new Instructor { Id = Guid.NewGuid(), FirstName = "Iris", LastName = "Instructor", Bio = "Senior coach.", MemberId = instructorUser.Id },
            new Instructor { Id = Guid.NewGuid(), FirstName = "John", LastName = "Smith", Bio = "Cycling specialist." },
            new Instructor { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Bio = "Yoga and Pilates teacher." },
        };
        await db.Instructors.AddRangeAsync(instructors, ct);

        var alice = await CreateUserAsync("alice@sportclub.test", "Alice", "Andrews", AuthRoles.Member, ct);
        var bob = await CreateUserAsync("bob@sportclub.test", "Bob", "Brown", AuthRoles.Member, ct);

        var now = DateTimeOffset.UtcNow;
        db.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            MemberId = alice.Id,
            Type = SubscriptionType.TwicePerWeek,
            StartUtc = now.AddDays(-30),
            EndUtc = now.AddMonths(6),
        });
        db.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            MemberId = bob.Id,
            Type = SubscriptionType.Yearly,
            StartUtc = now.AddDays(-30),
            EndUtc = now.AddDays(-30).AddYears(1),
        });

        var sessions = new List<ClassSession>();
        var startOfDay = new DateTimeOffset(DateOnly.FromDateTime(now.UtcDateTime).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var rotation = 0;
        for (var d = 0; d < 14; d++)
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
        await db.ClassSessions.AddRangeAsync(sessions, ct);

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded {Workouts} workouts, {Locations} locations, {Instructors} instructors, {Members} members, {Sessions} sessions.",
            workouts.Length, locations.Length, instructors.Length, 3, sessions.Count);
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
