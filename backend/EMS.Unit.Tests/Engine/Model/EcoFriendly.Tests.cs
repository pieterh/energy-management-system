using System;
using System.Diagnostics.CodeAnalysis;
using EMS.Engine;
using EMS.Engine.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Xunit;

namespace EMS.Tests.Engine.Model
{
    [SuppressMessage("Code Analyses","CA1707")]
	public class EcoFriendlyTests
	{
        [Fact]
        public void _010_HandlesMeasurementArrayNull()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");
            var mockStateMachine = new Mock<ChargingStateMachine>();
            var mock = new Mock<EcoFriendly>(MockBehavior.Strict, logger, new Measurements(10), mockStateMachine.Object) { CallBase = true };
            mock.SetupGet(p => p.MinimumDataPoints).Returns(10);
            mock.Setup(p => p.GetCurrent()).CallBase();
            mock.Object.GetCurrent().Should().Be((-1, -1, -1), "there are no or not enough samples");
        }

        [Fact]
        public void _020_HandlesLessThenMinimum()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");
            var mockStateMachine = new Mock<ChargingStateMachine>();
            var m = new Measurements(10);
            var mock = new Mock<EcoFriendly>(MockBehavior.Strict, logger, m, mockStateMachine.Object) { CallBase = true };
            
            mock.SetupGet(p => p.MinimumDataPoints).Returns(10);
            mock.Setup(p => p.GetCurrent()).CallBase();
            m.BufferSeconds = mock.Object.MinimumDataPoints;

            for (int i = 0; i < mock.Object.MinimumDataPoints - 1; i++)
                m.AddData(10, 0, 0, 10, 0, 0);

            mock.Object.GetCurrent().Should().Be((-1, -1, -1), "there are no or not enough samples");
        }

        [Fact]
        public void _030_Test2()
        {
            var logger = NullLoggerFactory.Instance.CreateLogger("nulllogger");
            var m = new Measurements(10);
            ChargingStateMachine state = new();
            var mock = new Mock<EcoFriendly>(MockBehavior.Strict, logger, m, state) { CallBase = true };
            mock.SetupGet(p => p.MinimumDataPoints).Returns(10);
            mock.SetupGet(p => p.MaxBufferSeconds).Returns(750);
            
            mock.Setup(p => p.GetCurrent()).CallBase();
            m.BufferSeconds = mock.Object.MinimumDataPoints;

            for (int i = 0; i < mock.Object.MinimumDataPoints; i++)
                m.AddData(-12.0, 0.62, 0.40,0, 0, 0);

            mock.Object.GetCurrent().Should().Be((10.83f, 0, 0));
        }
    }
}
