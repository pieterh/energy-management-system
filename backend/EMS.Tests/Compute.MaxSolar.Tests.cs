
using System.Collections.Generic;
using Xunit;
using Moq;
using FluentAssertions;

using EMS.Library;
using EMS.Library.Adapter;

namespace EMS.Tests
{
    public class ComputeMaxSolar
    {
        [Fact]
        public void HandlesMeasurementArrayNull()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);
            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        [Fact]
        public void HandlesLessThenMinimum()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            for (int i = 0; i < mock.Object.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(10, 0, 0), new CurrentMeasurement(10, 0, 0));

            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        [Fact]
        public void NotEnoughProduction1()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            mock.Object.AddMeasurement(new CurrentMeasurement(-3, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(-3, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(-3, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(-3, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(-3, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(-3, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(-3, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(-2, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(2, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(4, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(7, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(10, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(13, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(13, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(13, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(16f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(16f, 0f, 0f));

            mock.Object.Charging().Should().Be((7.36f, 0, 0));  //4A?
        }

        [Fact]
        public void NotEnoughProduction2()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            mock.Object.AddMeasurement(new CurrentMeasurement(3, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(3, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(3, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(3, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(3, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(3, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(3, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(2, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(2, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(4, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(7, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(10, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(13, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(13, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(13, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 0), new CurrentMeasurement(8.64f, 0f, 0f));
            mock.Object.AddMeasurement(new CurrentMeasurement(12, 1, 1), new CurrentMeasurement(8.64f, 0f, 0f));

            mock.Object.Charging().Should().Be((0f, 0, 0));
        }

        [Fact]
        public void Test2()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-12.0, 0.62, 0.40), new CurrentMeasurement(0f, 0f, 0f));

            mock.Object.Charging().Should().Be((10.98f, 0, 0));
        }


        [Theory]
        [MemberData(nameof(GetData), parameters: "_040_Test3")]
        public void _040_Test3(ICurrentMeasurement sm, ICurrentMeasurement evse, ICurrentMeasurement expected, string because)
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(sm, evse);

            mock.Object.Charging().Should().Be((expected.CurrentL1.Value, expected.CurrentL2.Value, expected.CurrentL3.Value), because);
        }

        public static IEnumerable<object[]> GetData(string testset)
        {
            var allData = new List<object[]>
            {
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(0, 0, 0), new CurrentMeasurement(0, 0, 0), "we are not charging and return is not enough to charge" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(6f, 0, 0), new CurrentMeasurement(8.17f, 0, 0), "we are charging at 6A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(8f, 0, 0), new CurrentMeasurement(10.17f, 0, 0), "we are charging at 8A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(10f, 0, 0), new CurrentMeasurement(12.17f, 0, 0), "we are charging at 10A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(12f, 0, 0), new CurrentMeasurement(14.17f, 0, 0), "we are charging at 12A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(14f, 0, 0), new CurrentMeasurement(16f, 0, 0), "we are charging at 14A, so we can increase charging to the max" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(16f, 0, 0), new CurrentMeasurement(16f, 0, 0), "we are charging at 16A, so we are already at the max" }
            };
            return allData;
        }
    }
}
