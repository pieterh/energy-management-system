﻿using System;
using System.Collections.Generic;
using System.Linq;
using EMS.Library.TestableDateTime;
using NCrontab;

namespace EMS.Library.Cron;

// Proxy.Wrapper for the NCrontab package

/// <summary>
/// </summary>
public class Crontab
{
    private readonly CrontabSchedule _cs;

    public Crontab(string expression, bool includeSeconds = false)
    {
        _cs = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions() { IncludingSeconds = includeSeconds });
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
        if ((occurrence - baseTime).TotalMilliseconds < 50)
        {
            List<System.DateTime> foundNext;
            DateTime begin = occurrence;
            DateTime end = begin;
            do
            {
                end = end.AddSeconds(10);
                foundNext = cs.GetNextOccurrences(begin, end).ToList();
            } while (!foundNext.Any());
            occurrence = foundNext[0];
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