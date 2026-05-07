using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Api.Extensions;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Api.Controllers;

[ApiController]
[Authorize(Roles = AuthRoles.Instructor)]
[Route("api/v1/instructors/me")]
public sealed class InstructorController(AppDbContext db) : ControllerBase
{
    [HttpGet("classes")]
    [ProducesResponseType(typeof(IReadOnlyList<ClassSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClassSessionDto>>> MyClasses(CancellationToken ct)
    {
        var memberId = User.GetMemberId();

        var instructor = await db.Instructors.SingleOrDefaultAsync(i => i.MemberId == memberId, ct);
        if (instructor is null)
        {
            return Ok(Array.Empty<ClassSessionDto>());
        }

        var sessions = await db.ClassSessions
            .AsNoTracking()
            .Include(c => c.Workout)
            .Include(c => c.Instructor)
            .Include(c => c.Location)
            .Where(c => c.InstructorId == instructor.Id)
            .OrderBy(c => c.StartUtc)
            .ToListAsync(ct);

        var result = new List<ClassSessionDto>(sessions.Count);
        foreach (var s in sessions)
        {
            var reserved = await db.Reservations.CountAsync(
                r => r.ClassSessionId == s.Id && r.Status == ReservationStatus.Active, ct);
            var waiting = await db.WaitingListEntries.CountAsync(w => w.ClassSessionId == s.Id, ct);
            var freeSpots = Math.Max(0, s.Capacity - reserved);

            result.Add(new ClassSessionDto(
                s.Id, s.StartUtc, s.Capacity, reserved, waiting, freeSpots, freeSpots == 0,
                new WorkoutDto(s.Workout.Id, s.Workout.Name, s.Workout.Description, s.Workout.DurationMinutes),
                new InstructorDto(s.Instructor.Id, s.Instructor.FirstName, s.Instructor.LastName, s.Instructor.Bio),
                new LocationDto(s.Location.Id, s.Location.Name, s.Location.Address)));
        }

        return Ok(result);
    }

    [HttpGet("classes/{classId:guid}/participants")]
    [ProducesResponseType(typeof(IReadOnlyList<ClassParticipantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ClassParticipantDto>>> Participants(Guid classId, CancellationToken ct)
    {
        var memberId = User.GetMemberId();

        var instructor = await db.Instructors.SingleOrDefaultAsync(i => i.MemberId == memberId, ct);
        if (instructor is null)
        {
            return Forbid();
        }

        var session = await db.ClassSessions.SingleOrDefaultAsync(c => c.Id == classId, ct);
        if (session is null)
        {
            return NotFound();
        }

        if (session.InstructorId != instructor.Id)
        {
            return Forbid();
        }

        var participants = await db.Reservations
            .AsNoTracking()
            .Where(r => r.ClassSessionId == classId && r.Status == ReservationStatus.Active)
            .OrderBy(r => r.CreatedUtc)
            .Select(r => new ClassParticipantDto(
                r.MemberId,
                r.Member.FirstName,
                r.Member.LastName,
                r.CreatedUtc))
            .ToListAsync(ct);

        return Ok(participants);
    }
}
