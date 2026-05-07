namespace SportclubApp.Api.Common;

public static class IsoWeek
{
    public static (DateTimeOffset Start, DateTimeOffset End) GetCurrentRange(DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var monday = today.AddDays(-daysSinceMonday);
        var nextMonday = monday.AddDays(7);

        return (
            new DateTimeOffset(monday.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new DateTimeOffset(nextMonday.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
    }
}
