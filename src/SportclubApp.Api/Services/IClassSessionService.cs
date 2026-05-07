using SportclubApp.Shared.Dtos;

namespace SportclubApp.Api.Services;

public interface IClassSessionService
{
    Task<IReadOnlyList<ClassSessionDto>> GetScheduleAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    Task<ClassSessionDto?> GetByIdAsync(Guid id, CancellationToken ct);
}
