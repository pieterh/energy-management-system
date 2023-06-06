using System;
using System.Collections.Generic;
using System.Linq;
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
        /* if the next occurence to close ( 50 ms) we will try to find a better one more in the future */
        if (occurrence.Ticks < 50000)
        {
            IEnumerable<System.DateTime> foundNext;
            DateTime begin;
            var end = occurrence;
            do
            {
                begin = end;
                end = begin.AddMinutes(30);
                foundNext = cs.GetNextOccurrences(begin, end).ToArray();
            } while (foundNext.Any());
            occurrence = foundNext.First();
        }
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