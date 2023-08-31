using System;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using Moq;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using static EMS.Compute;
using EMS.Library.Adapter;
using EMS.Library.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EMS.Tests
{
    [SuppressMessage("","S125")]
    public class ComputeMaxCharge
    {
        [Fact]
        public void MaxChargeHandlesMeasurementArrayNull()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");

            var mock = new Mock<Compute>(logger, ChargingMode.MaxCharge);
            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        [Fact]
        public void MaxChargeHandlesLessThenMinimum()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");

            var mock = new Mock<Compute>(logger, ChargingMode.MaxCharge);

            for (int i = 0; i < mock.Object.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new CurrentMeasurement(10, 0, 0), new CurrentMeasurement(10f, 0f, 0f));

            mock.Object.Charging().Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        //[Fact]
        //public void MaxChargeNotChargingYet()
        //{
        //    var mock = new Mock<Compute>(null, ChargingMode.MaxCharge);

        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.778, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.778, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.76, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.76, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.761, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.761, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.781, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.764, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.765, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.765, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.765, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.767, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.768, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.767, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.77, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.772, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.771, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.774, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0.774, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0.774, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.773, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.772, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0.764, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.734, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.772, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(1, 1, 9, 0.229, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(2, 1, 9, 0, 0, 0));
        //    mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0.785, 0, 0));

        //    mock.Object.Charging(new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f), "Because that is the maximum we can charge");
        //}

        [Fact]
        public void MaxChargingTest1()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");
            var mock = new Mock<Compute>(logger, ChargingMode.MaxCharge);

            AddSamplesLoadSinglePhase1(mock.Object, new CurrentMeasurement(16f, 0f, 0f));

            mock.Object.Charging().Should().Be((16f, 16f, 16f));
        }

        [Theory]
        [InlineData(0f, 0f, 0f, 7.43f, 16f, 16f, "")]
        [InlineData(4f, 0f, 0f, 11.43f, 16f, 16f, "")]
        [InlineData(8f, 0f, 0f, 15.43f, 16f, 16f, "")]
        [InlineData(9f, 0f, 0f, 16f, 16f, 16f, "")]
        [InlineData(10f, 0f, 0f, 16f, 16f, 16f, "")]
        public void MaxChargingTest2(double cl1, double cl2, double cl3, double el1, double el2, double el3, string because)
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");
            var mock = new Mock<Compute>(logger, ChargingMode.MaxCharge);

            AddSamplesLoadSinglePhase2(mock.Object, new CurrentMeasurement(cl1, cl2, cl3));

            mock.Object.Charging().Should().Be((el1, el2, el3), because);
        }

        [Theory]
        [InlineData(16f, 16f, 16f, 16f, 16f, 16f, "")]
        [InlineData(10f, 10f, 10f, 16f, 16f, 16f, "")]
        [InlineData(8f, 8f, 8f, 14f, 16f, 16f, "")]
        [InlineData(6f, 6f, 6f, 12f, 16f, 16f, "")]
        public void MaxChargingHandlesLoad(double cl1, double cl2, double cl3, double el1, double el2, double el3, string because)
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");
            var mock = new Mock<Compute>(logger, ChargingMode.MaxCharge);

            AddSamplesLoad(mock.Object, new CurrentMeasurement(cl1, cl2, cl3));
            mock.Object.Charging().Should().Be((el1, el2, el3), because);
        }

        [Theory]
        [InlineData(16f, 16f, 16f, 16f, 16f, 0f, "")]
        [InlineData(10f, 10f, 10f, 16f, 16f, 0f, "")]
        public void MaxChargingHandlesOverloading(double cl1, double cl2, double cl3, double el1, double el2, double el3, string because)
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");
            var mock = new Mock<Compute>(logger, ChargingMode.MaxCharge);

            AddSamplesOverload(mock.Object, new CurrentMeasurement(cl1, cl2, cl3));
            mock.Object.Charging().Should().Be((el1, el2, el3), because);
        }

        protected static void AddSamplesLoadSinglePhase1(Compute c, ICurrentMeasurement charge)
        {
            ArgumentNullException.ThrowIfNull(c);
            c.AddMeasurement(new CurrentMeasurement(3, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(3, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(3, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(3, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(3, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(3, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(3, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(2, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(2, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(4, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(7, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(10, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(13, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(13, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(13, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 1), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 0), charge);
            c.AddMeasurement(new CurrentMeasurement(12, 1, 1), charge);
        }

        protected static void AddSamplesLoadSinglePhase2(Compute c, ICurrentMeasurement charge)
        {
            ArgumentNullException.ThrowIfNull(c);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(18, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
            c.AddMeasurement(new CurrentMeasurement(17, 1, 5), charge);
        }

        protected static void AddSamplesLoad(Compute c, ICurrentMeasurement charge)
        {
            ArgumentNullException.ThrowIfNull(c);
            for (int i = 0; i < c.MinimumDataPoints; i++)
                c.AddMeasurement(new CurrentMeasurement(19, 11, 11), charge);
        }

        protected static void AddSamplesOverload(Compute c, ICurrentMeasurement charge)
        {
            ArgumentNullException.ThrowIfNull(c);
            for (int i = 0; i < c.MinimumDataPoints; i++)
                c.AddMeasurement(new CurrentMeasurement(19, 11, 39), charge);
        }
    }
}
