using System;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using Moq;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using static EMS.Compute;

namespace EMS.Test
{
    public class ComputeMaxSolar
    {
        [Fact]
        public void HandlesMeasurementArrayNull()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);
            mock.Object.Charging(new ChargingInfo(10, 0, 0, 235, 235, 235)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }
        [Fact]
        public void HandlesLessThenMinimum()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            for (int i = 0; i < Compute.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new MeasurementBase(10, 0, 0, 235, 235, 235));

            mock.Object.Charging(new ChargingInfo(10, 0, 0, 235, 235, 235)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }
        [Fact]
        public void NotEnoughProduction1()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            mock.Object.AddMeasurement(new MeasurementBase(-3, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(-3, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(-3, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(-3, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(-3, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(-3, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(-3, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(-2, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(4, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(7, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(10, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 235, 235, 235));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 235, 235, 235));

            mock.Object.Charging(new ChargingInfo(16f, 0f, 0f, 230f, 230f, 230f)).Should().Be((7.36f, 0, 0));  //4A?
        }
        [Fact]
        public void NotEnoughProduction2()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 0, 0.755, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0.754, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0.752, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0.75, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0.746, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0.701, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0.715, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 1, 0.375, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(4, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(7, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(10, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));

            mock.Object.Charging(new ChargingInfo(8.64f, 0f, 0f, 230, 230, 230)).Should().Be((0f, 0, 0));
        }

        [Fact]
        public void Test2()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            for (int i = 0; i < Compute.MinimumDataPoints ; i++)
                mock.Object.AddMeasurement(new MeasurementBase(-12.0, 0.62, 0.40, 235, 235, 235));

            mock.Object.Charging(new ChargingInfo(0f, 0f, 0f, 235, 235, 235)).Should().Be((10.98f, 0, 0));
        }
        [Fact]
        public void Test3()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxSolar);

            for (int i = 0; i < Compute.MinimumDataPoints; i++)
                mock.Object.AddMeasurement(new MeasurementBase(-3.19, 0.62, 0.40, 235, 235, 235));

            mock.Object.Charging(new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((0.0f, 0, 0), "we are not charging and return is not enough to charge");
            mock.Object.Charging(new ChargingInfo(6f, 0f, 0f, 230f, 230f, 230f)).Should().Be((8.17f, 0, 0), "we are charging at 6A, so we can increase charging");
            mock.Object.Charging(new ChargingInfo(8f, 0f, 0f, 230f, 230f, 230f)).Should().Be((10.17f, 0, 0), "we are charging at 8A, so we can increase charging");
            mock.Object.Charging(new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((12.17f, 0, 0), "we are charging at 10A, so we can increase charging");
            mock.Object.Charging(new ChargingInfo(12f, 0f, 0f, 230f, 230f, 230f)).Should().Be((14.17f, 0, 0), "we are charging at 12A, so we can increase charging");
            mock.Object.Charging(new ChargingInfo(14f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16.0f, 0, 0), "we are charging at 14A, so we can increase charging to the max");
            mock.Object.Charging(new ChargingInfo(16f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16.0f, 0, 0), "we are charging at 16A, so we are already at the max");
        }
    }

    public class ComputeMaxCharge
    {

        [Fact]
        public void MaxChargeHandlesMeasurementArrayNull()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);
            mock.Object.Charging(new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }
        [Fact]
        public void MaxChargeHandlesLessThenMinimum()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);

            for (int i = 0; i < Compute.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new MeasurementBase(10, 0, 0, 0, 0, 0));

            mock.Object.Charging(new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        [Fact]
        public void MaxChargeNotChargingYet()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);

            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.778, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.778, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.76, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.76, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.761, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.761, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.781, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.764, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.765, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.765, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.765, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.767, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.768, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.767, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.77, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.772, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.771, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.774, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0.774, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0.774, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.773, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.772, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0.764, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.734, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.772, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(1, 1, 9, 0.229, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 9, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.785, 0, 0));

            mock.Object.Charging(new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f), "Because that is the maximum we can charge");
        }

        [Fact]
        public void MaxChargingTest1()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);

            AddSamplesLoadSinglePhase1(mock.Object);

            mock.Object.Charging(new ChargingInfo(16f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f));
        }

        [Fact]
        public void MaxChargingTest2()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);

            AddSamplesLoadSinglePhase2(mock.Object);

            mock.Object.Charging(new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((7.43f, 16f, 16f));
            mock.Object.Charging(new ChargingInfo(4f, 0f, 0f, 230f, 230f, 230f)).Should().Be((11.43f, 16f, 16f));
            mock.Object.Charging(new ChargingInfo(8f, 0f, 0f, 230f, 230f, 230f)).Should().Be((15.43f, 16f, 16f));
            mock.Object.Charging(new ChargingInfo(9f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f));
            mock.Object.Charging(new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f));
        }

        [Fact]
        public void MaxChargingHandlesLoad()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);

            AddSamplesLoad(mock.Object);
            mock.Object.Charging(new ChargingInfo(16f, 16f, 16f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f), "a");
            mock.Object.Charging(new ChargingInfo(10f, 10f, 10f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f), "b");
            mock.Object.Charging(new ChargingInfo(8f, 8f, 8f, 230f, 230f, 230f)).Should().Be((14f, 16f, 16f), "c");
            mock.Object.Charging(new ChargingInfo(6f, 6f, 6f, 230f, 230f, 230f)).Should().Be((12f, 16f, 16f), "d");
        }

        [Fact]
        public void MaxChargingHandlesOverloading()
        {
            var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);

            AddSamplesOverload(mock.Object);
            mock.Object.Charging(new ChargingInfo(16f, 16f, 16f, 230f, 230f, 230f)).Should().Be((16f, 16f, 0f));
            mock.Object.Charging(new ChargingInfo(10f, 10f, 10f, 230f, 230f, 230f)).Should().Be((16f, 16f, 0f));
        }


        protected static void AddSamplesLoadSinglePhase1(Compute c)
        {
            c.AddMeasurement(new MeasurementBase(3, 1, 0, 0.755, 0, 0));
            c.AddMeasurement(new MeasurementBase(3, 1, 1, 0.754, 0, 0));
            c.AddMeasurement(new MeasurementBase(3, 1, 1, 0.752, 0, 0));
            c.AddMeasurement(new MeasurementBase(3, 1, 1, 0.75, 0, 0));
            c.AddMeasurement(new MeasurementBase(3, 1, 1, 0.746, 0, 0));
            c.AddMeasurement(new MeasurementBase(3, 1, 1, 0.701, 0, 0));
            c.AddMeasurement(new MeasurementBase(3, 1, 1, 0.715, 0, 0));
            c.AddMeasurement(new MeasurementBase(2, 1, 1, 0.375, 0, 0));
            c.AddMeasurement(new MeasurementBase(2, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(4, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(7, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(10, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(13, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(13, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(13, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 0, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(12, 1, 1, 0, 0, 0));
        }

        protected static void AddSamplesLoadSinglePhase2(Compute c)
        {
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(18, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
            c.AddMeasurement(new MeasurementBase(17, 1, 5, 0, 0, 0));
        }

        protected static void AddSamplesLoad(Compute c)
        {
            for (int i = 0; i < Compute.MinimumDataPoints; i++)
                c.AddMeasurement(new MeasurementBase(19, 11, 11, 0, 0, 0));
        }

        protected static void AddSamplesOverload(Compute c)
        {
            for (int i = 0; i < Compute.MinimumDataPoints; i++)
                c.AddMeasurement(new MeasurementBase(19, 11, 39, 0, 0, 0));
        }


    }
}
