using NCrontab;

namespace EMS.Library.Cron;

// Proxy.Wrapper for the NCrontab package

/// <summary>
/// </summary>
public class Crontab
{
    private readonly CrontabSchedule _cs;

    public Crontab(string expression)
    {
        _cs = CrontabSchedule.Parse(expression);
    }

    public DateTimeOffset GetNextOccurrence(DateTimeOffset baseTime)
    {
        return GetNextOccurrence(_cs, baseTime);
    }

    public IEnumerable<DateTimeOffset> GetNextOccurrences(DateTimeOffset start, DateTimeOffset end)
    {
        return GetNextOccurrences(_cs, start, end);
    }

    internal static DateTimeOffset GetNextOccurrence(CrontabSchedule cs, DateTimeOffset baseTime)
    {
        ArgumentNullException.ThrowIfNull(cs);
        var occurrence = cs.GetNextOccurrence(baseTime.UtcDateTime);
        return new DateTimeOffset(occurrence);
    }

    internal static IEnumerable<DateTimeOffset> GetNextOccurrences(CrontabSchedule cs, DateTimeOffset start, DateTimeOffset end)
    {
        ArgumentNullException.ThrowIfNull(cs);
        var occurrences = cs.GetNextOccurrences(start.UtcDateTime, end.UtcDateTime);
        var r = occurrences.Select((x) => new DateTimeOffset(x));
        return r;
    }
}