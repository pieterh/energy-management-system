using FluentAssertions;
using EMS.Library.Cron;

namespace CrontabUnitTest;

public class CrontabTests
{
    [Fact]
    public void BasicTestGetNextOccurrence()
    {
        var crontab = new Crontab("55 * * * *");
        var baseTime = new DateTimeOffset(2023, 05, 1, 13, 0, 0, new TimeSpan(1, 0, 0));
        var next = crontab.GetNextOccurrence(baseTime);
        next.Year.Should().Be(2023);
        next.Month.Should().Be(5);
        next.Day.Should().Be(1);
        next.Hour.Should().Be(12);
        next.Minute.Should().Be(55);
        next.Second.Should().Be(0);
    }

    [Fact]
    public void BasicTestGetNextOccurrences()
    {
        var crontab = new Crontab("55 * * * *");
        var start = new DateTimeOffset(2023, 05, 1, 13, 0, 0, new TimeSpan(1, 0, 0));
        var end = new DateTimeOffset(2023, 05, 1, 15, 0, 0, new TimeSpan(1, 0, 0));
        var nextOccurences = crontab.GetNextOccurrences(start, end).ToArray();
        nextOccurences.Should().HaveCount(2);

        var next = nextOccurences[0];
        next.Year.Should().Be(2023);
        next.Month.Should().Be(5);
        next.Day.Should().Be(1);
        next.Hour.Should().Be(12);
        next.Minute.Should().Be(55);
        next.Second.Should().Be(0);

        var second = nextOccurences[1];
        second.Year.Should().Be(2023);
        second.Month.Should().Be(5);
        second.Day.Should().Be(1);
        second.Hour.Should().Be(13);
        second.Minute.Should().Be(55);
        second.Second.Should().Be(0);
    }
}