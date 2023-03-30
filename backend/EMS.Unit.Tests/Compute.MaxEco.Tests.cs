using System;
using System.Collections.Generic;

using Xunit;
using Moq;
using FluentAssertions;

using EMS.Engine;
using EMS.Library;
using EMS.Library.TestableDateTime;
using EMS.Library.Adapter;
using static EMS.Compute;
using EMS.Library.Core;
using EMS.Engine.Model;

namespace EMS.Tests
{
    sealed class CurrentMeasurement : ICurrentMeasurement
    {
        public double? CurrentL1 { get; set; }
        public double? CurrentL2 { get; set; }
        public double? CurrentL3 { get; set; }

        public CurrentMeasurement(double l1, double l2, double l3)
        {
            CurrentL1 = l1;
            CurrentL2 = l2;
            CurrentL3 = l3;
        }
    }

    public class ComputeMaxEco
    {
        [Fact(DisplayName = "010 HandlesMeasurementArrayNull")]
        public void T010HandlesMeasurementArrayNull()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);
            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be returned when there are no or not enough samples");
        }

        [Fact(DisplayName = "020 HandlesLessThenMinimum")]
        public void T020HandlesLessThenMinimum()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(10, 0, 0), new CurrentMeasurement(10, 0, 0));

            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        [Fact(DisplayName = "030 Test2")]
        public void T030Test2()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-12.0, 0.62, 0.40), new CurrentMeasurement(0, 0, 0));

            mock.Object.Charging().Should().Be((10.83f, 0, 0));
        }

        [Theory(DisplayName = "040 Test3")]
        [MemberData(nameof(GetData), parameters: "_040_Test3")]
        public void T040Test3(ICurrentMeasurement sm, ICurrentMeasurement evse, ICurrentMeasurement expected, string because)
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(sm, evse);

            var l1 = expected?.CurrentL1 ?? double.NaN;
            var l2 = expected?.CurrentL2 ?? double.NaN;
            var l3 = expected?.CurrentL3 ?? double.NaN;
            mock.Object.Charging().Should().Be((l1, l2, l3), because);
        }

        public static IEnumerable<object[]> GetData(string testset)
        {
            var allData = new List<object[]>
            {
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(0, 0, 0), new CurrentMeasurement(0, 0, 0), "we are not charging and return is not enough to charge" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(6f, 0, 0), new CurrentMeasurement(8.02f, 0, 0), "we are charging at 6A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(8f, 0, 0), new CurrentMeasurement(10.02f, 0, 0), "we are charging at 8A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(10f, 0, 0), new CurrentMeasurement(12.02f, 0, 0), "we are charging at 10A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(12f, 0, 0), new CurrentMeasurement(14.02f, 0, 0), "we are charging at 12A, so we can increase charging" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(14f, 0, 0), new CurrentMeasurement(15.85f, 0, 0), "we are charging at 14A, so we can increase charging to the max" },
                new object[] { new CurrentMeasurement(-3.19, 0.62, 0.40), new CurrentMeasurement(16f, 0, 0), new CurrentMeasurement(15.85f, 0, 0), "we are charging at 16A, so we are already at the max" }
            };
            return allData;
        }

        [Fact(DisplayName = "050 StartChargeThreshold_before_no_charge")]
        public void T050StartChargeThresholdBeforeNoCharge()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-4.01, 0.62, 0.40), new CurrentMeasurement(0f, 0f, 0f));

            mock.Object.Charging().Should().Be((0, 0, 0), "we have not enough energy return to start charging");
        }

        [Fact(DisplayName = "060 StartChargeThreshold_at_start_charge")]
        public void T060StartChargeThresholdAtStartCharge()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-7.77, 0.62, 0.40), new CurrentMeasurement(0f, 0f, 0f));

            mock.Object.Charging().Should().Be((6.6f, 0, 0), "we reached minimum energy return, so we can start charging");
        }

        [Fact(DisplayName = "070 StartChargeThreshold_after_good_charge")]
        public void T070StartChargeThresholdAfterGoodCharge()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-8.02, 0.62, 0.40), new CurrentMeasurement(0f, 0f, 0f));

            mock.Object.Charging().Should().Be((6.85f, 0, 0), "we have more than enough energy return available so that we can increase above minimum");
        }
    }
}
