using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Entities;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Services;

public sealed class ClassSessionService(AppDbContext db) : IClassSessionService
{
    public async Task<IReadOnlyList<ClassSessionDto>> GetScheduleAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        var sessions = await db.ClassSessions
            .AsNoTracking()
            .Include(c => c.Workout)
            .Include(c => c.Instructor)
            .Include(c => c.Location)
            .Where(c => c.StartUtc >= from && c.StartUtc < to)
            .OrderBy(c => c.StartUtc)
            .ToListAsync(ct);

        if (sessions.Count == 0)
        {
            return [];
        }

        var sessionIds = sessions.Select(s => s.Id).ToList();

        var reservedCounts = await db.Reservations
            .Where(r => sessionIds.Contains(r.ClassSessionId) && r.Status == ReservationStatus.Active)
            .GroupBy(r => r.ClassSessionId)
            .Select(g => new { ClassSessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassSessionId, x => x.Count, ct);

        var waitingCounts = await db.WaitingListEntries
            .Where(w => sessionIds.Contains(w.ClassSessionId))
            .GroupBy(w => w.ClassSessionId)
            .Select(g => new { ClassSessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassSessionId, x => x.Count, ct);

        return sessions
            .Select(s => Map(s, reservedCounts.GetValueOrDefault(s.Id), waitingCounts.GetValueOrDefault(s.Id)))
            .ToList();
    }

    public async Task<ClassSessionDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var session = await db.ClassSessions
            .AsNoTracking()
            .Include(c => c.Workout)
            .Include(c => c.Instructor)
            .Include(c => c.Location)
            .SingleOrDefaultAsync(c => c.Id == id, ct);

        if (session is null)
        {
            return null;
        }

        var reserved = await db.Reservations
            .CountAsync(r => r.ClassSessionId == id && r.Status == ReservationStatus.Active, ct);

        var waiting = await db.WaitingListEntries.CountAsync(w => w.ClassSessionId == id, ct);

        return Map(session, reserved, waiting);
    }

    private static ClassSessionDto Map(ClassSession session, int reserved, int waiting)
    {
        var freeSpots = Math.Max(0, session.Capacity - reserved);
        return new ClassSessionDto(
            Id: session.Id,
            StartUtc: session.StartUtc,
            Capacity: session.Capacity,
            ReservedCount: reserved,
            WaitingListCount: waiting,
            FreeSpots: freeSpots,
            IsFull: freeSpots == 0,
            Workout: new WorkoutDto(session.Workout.Id, session.Workout.Name, session.Workout.Description, session.Workout.DurationMinutes),
            Instructor: new InstructorDto(session.Instructor.Id, session.Instructor.FirstName, session.Instructor.LastName, session.Instructor.Bio),
            Location: new LocationDto(session.Location.Id, session.Location.Name, session.Location.Address));
    }
}
