using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceRecordDto>> GetHistoryAsync(Guid memberId, int year, CancellationToken ct);
}
