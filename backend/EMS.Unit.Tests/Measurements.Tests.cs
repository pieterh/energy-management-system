
using Xunit;
using FluentAssertions;
using EMS.Engine;
using EMS.Library.TestableDateTime;

namespace EMS.Tests
{
    public class MeasurementsTests
    {
        [Fact]
        public void ShouldNotKeepDataTooLongInTheBuffer()
        {
            const int bufferseconds = 100;

            var mock = new Mock<Measurements>(bufferseconds);

            // start filling buffer with some data, but don't overfill the buffer
            var dateTime = new DateTime(2021, 05, 01, 13, 15, 0, DateTimeKind.Utc);
            for (int i = 0; i < bufferseconds - 10; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(0, 0, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(1);
            }

            var r1 = mock.Object.CalculateAverageUsage();

            r1.NrOfDataPoints.Should().Be(bufferseconds - 10, "all items till now added, should now be in the buffer");

            // add some more data to the buffer
            for (int i = 0; i < bufferseconds * 2; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(0, 0, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(1);
            }
            var r2 = mock.Object.CalculateAverageUsage();
            r2.NrOfDataPoints.Should().Be(bufferseconds , "should not exceed the maximum");
        }

        [Fact]
        public void ShouldCalculateAverageUsage()
        {
            const int bufferseconds = 100;

            var mock = new Mock<Measurements>(bufferseconds);

            // start filling buffer halfway 
            var dateTime = new DateTime(2021, 05, 01, 13, 15, 0, DateTimeKind.Utc);
            for (int i = 0; i < bufferseconds / 2; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(1, 2, 3, 0, 0, 0);
                dateTime = dateTime.AddSeconds(1);
            }
            // fill the second part of the buffer
            for (int i = 0; i < bufferseconds / 2; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(3, 4, 5, 0, 0, 0);
                dateTime = dateTime.AddSeconds(1);
            }

            // we know what the average now should be
            var r1 = mock.Object.CalculateAverageUsage();
            r1.NrOfDataPoints.Should().Be(bufferseconds, $"we have added {bufferseconds} times every second a value");
            r1.CurrentUsingL1.Should().Be(2, "it is the average between 1 and 3");
            r1.CurrentUsingL2.Should().Be(3, "it is the average between 2 and 4");
            r1.CurrentUsingL3.Should().Be(4, "it is the average between 3 and 5");

            r1.CurrentChargingL1.Should().Be(0, "we are not charging");
            r1.CurrentChargingL2.Should().Be(0, "we are not charging");
            r1.CurrentChargingL3.Should().Be(0, "we are not charging");

            // overflow the buffer with zero values
            for (int i = 0; i < bufferseconds; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(0, 0, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(1);
            }

            // since the buffer is now filled with zero's, lets check if the average does agree ;-)
            var r2 = mock.Object.CalculateAverageUsage();

            r2.NrOfDataPoints.Should().Be(bufferseconds, $"we have added {bufferseconds} times every second a value");

            r2.CurrentUsingL1.Should().Be(0, "since we filled the buffer with all 0 values");
            r2.CurrentUsingL2.Should().Be(0, "since we filled the buffer with all 0 values");
            r2.CurrentUsingL3.Should().Be(0, "since we filled the buffer with all 0 values");
            r2.CurrentChargingL1.Should().Be(0, "we are not charging");
            r2.CurrentChargingL2.Should().Be(0, "we are not charging");
            r2.CurrentChargingL3.Should().Be(0, "we are not charging");
        }

        [Fact]
        public void ShouldCalculateAverageCharging()
        {
            const int bufferseconds = 100;

            var mock = new Mock<Measurements>(bufferseconds);

            // start filling buffer halfway 
            var dateTime = new DateTime(2021, 05, 01, 13, 15, 0, DateTimeKind.Utc);
            for (int i = 0; i < bufferseconds / 2; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(0, 0, 0, 1, 2, 3);
                dateTime = dateTime.AddSeconds(1);
            }
            // fill the second part of the buffer
            for (int i = 0; i < bufferseconds / 2; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(0, 0, 0, 3, 4, 5);
                dateTime = dateTime.AddSeconds(1);
            }

            // we know what the average now should be
            var r1 = mock.Object.CalculateAverageUsage();
            r1.NrOfDataPoints.Should().Be(bufferseconds, $"we have added {bufferseconds} times every second a value");
            r1.CurrentUsingL1.Should().Be(0, "we are not using");
            r1.CurrentUsingL2.Should().Be(0, "we are not using");
            r1.CurrentUsingL3.Should().Be(0, "we are not using");

            r1.CurrentChargingL1.Should().Be(2, "it is the average between 1 and 3");
            r1.CurrentChargingL2.Should().Be(3, "it is the average between 2 and 4");
            r1.CurrentChargingL3.Should().Be(4, "it is the average between 3 and 5");


            // overflow the buffer with zero values
            for (int i = 0; i < bufferseconds; i++)
            {
                using (new DateTimeProviderContext(dateTime))
                    mock.Object.AddData(0, 0, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(1);
            }

            // since the buffer is now filled with zero's, lets check if the average does agree ;-)
            var r2 = mock.Object.CalculateAverageUsage();

            r2.NrOfDataPoints.Should().Be(bufferseconds, $"we have added {bufferseconds} times every second a value");


            r2.CurrentUsingL1.Should().Be(0, "we are not using");
            r2.CurrentUsingL2.Should().Be(0, "we are not using");
            r2.CurrentUsingL3.Should().Be(0, "we are not using");
            r2.CurrentChargingL1.Should().Be(0, "since we filled the buffer with all 0 values");
            r2.CurrentChargingL2.Should().Be(0, "since we filled the buffer with all 0 values");
            r2.CurrentChargingL3.Should().Be(0, "since we filled the buffer with all 0 values");
        }
    }
}
