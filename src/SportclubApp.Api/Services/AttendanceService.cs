using Microsoft.EntityFrameworkCore;
using SportclubApp.Api.Data;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public sealed class AttendanceService(AppDbContext db) : IAttendanceService
{
    public async Task<IReadOnlyList<AttendanceRecordDto>> GetHistoryAsync(Guid memberId, int year, CancellationToken ct)
    {
        var start = new DateTimeOffset(new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var end = start.AddYears(1);

        return await db.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.MemberId == memberId && a.AttendedUtc >= start && a.AttendedUtc < end)
            .OrderByDescending(a => a.AttendedUtc)
            .Select(a => new AttendanceRecordDto(
                a.Id,
                a.ClassSessionId,
                a.AttendedUtc,
                a.ClassSession.Workout.Name))
            .ToListAsync(ct);
    }
}
