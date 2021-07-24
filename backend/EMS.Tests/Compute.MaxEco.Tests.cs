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
    class CurrentMeasurement : ICurrentMeasurement
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
        [Fact]
        public void _010_HandlesMeasurementArrayNull()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);
            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be returned when there are no or not enough samples");
        }

        [Fact]
        public void _020_HandlesLessThenMinimum()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(10, 0, 0), new CurrentMeasurement(10, 0, 0));

            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        [Fact]
        public void _030_Test2()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-12.0, 0.62, 0.40), new CurrentMeasurement(0, 0, 0));

            mock.Object.Charging().Should().Be((10.83f, 0, 0));
        }

        [Theory]
        [MemberData(nameof(GetData), parameters: "_040_Test3")]
        public void _040_Test3(ICurrentMeasurement sm, ICurrentMeasurement evse, ICurrentMeasurement expected, string because)
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(sm, evse);

            mock.Object.Charging().Should().Be((expected.CurrentL1.Value, expected.CurrentL2.Value, expected.CurrentL3.Value), because);
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


        [Fact]
        public void _050_StartChargeThreshold_before_no_charge()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-4.01, 0.62, 0.40), new CurrentMeasurement(0f, 0f, 0f));

            mock.Object.Charging().Should().Be((0, 0, 0), "we have not enough energy return to start charging");
        }

        [Fact]
        public void _060_StartChargeThreshold_at_start_charge()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-7.02, 0.62, 0.40), new CurrentMeasurement(0f, 0f, 0f));

            mock.Object.Charging().Should().Be((6.0f, 0, 0), "we reached minimum energy return, so we can start charging");
        }

        [Fact]
        public void _070_StartChargeThreshold_after_good_charge()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(-8.02, 0.62, 0.40), new CurrentMeasurement(0f, 0f, 0f));

            mock.Object.Charging().Should().Be((6.85f, 0, 0), "we have more than enough energy return available so that we can increase above minimum");
        }

        //[Fact]
        //public void _080_HandlesTransition()
        //{
        //    var mock = new Mock<Compute>(null, ChargingMode.MaxEco);

        //    // start filling buffer with data that will not allow ecomode to charge the car
        //    var dateTime = new DateTime(2021, 05, 01, 13, 15, 0);
        //    for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
        //    {
        //        using (new DateTimeProviderContext(dateTime))
        //        {
        //            mock.Object.AddMeasurement(new CurrentMeasurement(1 - mock.Object.MinimumEcoModeExportStop, 0, 0), new CurrentMeasurement(0f, 0f, 0f));
        //        }
        //        dateTime = dateTime.AddSeconds(1);
        //    }
        //    using (new DateTimeProviderContext(dateTime))
        //        mock.Object.Charging().Should().Be((0, 0, 0), "we have not yet enough energy return to start charging");

        //    // add data to the buffer so that we have enough overcapacity to allow the car to charge
        //    for (int i = 0; i < mock.Object.MinimumDataPoints / 3; i++)
        //    {
        //        using (new DateTimeProviderContext(dateTime))
        //        {
        //            mock.Object.AddMeasurement(new CurrentMeasurement(-15 - mock.Object.MinimumEcoModeExportStop, 0, 0), new CurrentMeasurement(0f, 0f, 0f));
        //        }
        //        dateTime = dateTime.AddSeconds(1);
        //    }

        //    using (new DateTimeProviderContext(dateTime))
        //        mock.Object.Charging().Should().Be((15.85f, 0, 0), "we have now enough overcapicity and we take the last 15 seconds to determine the charge current");

        //    // there are some clouds ;-) so add some data with no return
        //    for (int i = 0; i < mock.Object.MinimumDataPoints / 4; i++)
        //    {
        //        using (new DateTimeProviderContext(dateTime))
        //        {
        //            mock.Object.AddMeasurement(new CurrentMeasurement(0, 0, 0), new CurrentMeasurement(0f, 0f, 0f));
        //        }
        //        dateTime = dateTime.AddSeconds(1);
        //    }
        //    using (new DateTimeProviderContext(dateTime))
        //        mock.Object.Charging().Should().Be((6, 0, 0), "not enough overcapicity, but switched recently ");

        //    // more clouds for 5 minutes
        //    for (int i = 0; i < 300; i++)
        //    {
        //        using (new DateTimeProviderContext(dateTime))
        //        {
        //            mock.Object.AddMeasurement(new CurrentMeasurement(1, 0, 0), new CurrentMeasurement(0f, 0f, 0f));
        //        }
        //        dateTime = dateTime.AddSeconds(1);
        //    }

        //    using (new DateTimeProviderContext(dateTime))
        //        mock.Object.Charging().Should().Be((0, 0, 0), "not enough overcapicity we shoudl stop");

        //    // there is the sun very bright for 5 minutes
        //    for (int i = 0; i < 300; i++)
        //    {
        //        using (new DateTimeProviderContext(dateTime))
        //        {
        //            mock.Object.AddMeasurement(new CurrentMeasurement(-12.5f, 0f, 0f), new CurrentMeasurement(0f, 0f, 0f));
        //        }
        //        dateTime = dateTime.AddSeconds(1);
        //    }

        //    using (new DateTimeProviderContext(dateTime))
        //        mock.Object.Charging().Should().Be((12.35f, 0, 0), "enough overcapicity, and 15 seconds is used");

        //    // there is still some sun for the second half minimum time
        //    for (int i = 0; i < ChargingStateMachine.DEFAULT_MINIMUM_TRANSITION_TIME / 2; i++)
        //    {
        //        using (new DateTimeProviderContext(dateTime))
        //        {
        //            mock.Object.AddMeasurement(new CurrentMeasurement(0 - mock.Object.MinimumEcoModeExportStop, 0, 0), new CurrentMeasurement(0f, 0f, 0f));
        //        }
        //        dateTime = dateTime.AddSeconds(1);
        //    }

        //    using (new DateTimeProviderContext(dateTime))
        //        mock.Object.Charging().Should().Be((6, 0, 0), "enough overcapicity and transition was enough time in the past");
        //}
    }
}
