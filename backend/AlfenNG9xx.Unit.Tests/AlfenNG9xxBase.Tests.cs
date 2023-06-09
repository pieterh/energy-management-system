using AlfenNG9xx;
using AlfenNG9xx.Tests;
using EMS.Library;
using EMS.Library.Configuration;
using Moq;
using Moq.Protected;

namespace AlfenNG9xxBase.Tests
{
    public class AlfenBaseTests
    {
        [Fact]
        [SuppressMessage("", "S1215")]
        public void DisposesProperly()
        {
            var w = new Mock<IWatchdog>();
            var mock = new Mock<AlfenNG9xx.AlfenBase>(new InstanceConfiguration(), new TestPriceProvider(), w.Object);
            mock.CallBase = true;

            mock.Object.Disposed.Should().BeFalse();
            _ = mock.Object.ReadProductInformation();

            mock.Object.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            mock.Object.Disposed.Should().BeTrue();
        }

        [Fact]
        [SuppressMessage("", "S1215")]
        public void DisposesCanSafelyCalledTwice()
        {
            var w = new Mock<IWatchdog>();
            var mock = new Mock<AlfenNG9xx.AlfenBase>(new InstanceConfiguration(), new TestPriceProvider(), w.Object);
            mock.CallBase = true;

            mock.Object.Disposed.Should().BeFalse();
            _ = mock.Object.ReadProductInformation();

            mock.Object.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            mock.Object.Disposed.Should().BeTrue();

            // and for the second time
            mock.Object.Dispose();
            mock.Object.Disposed.Should().BeTrue();
        }


        [Fact]
        public async Task HandleWorkShouldStopProperly()
        {
            var w = new Mock<IWatchdog>();
            var mockAlfen = new Mock<AlfenNG9xx.AlfenBase>(new InstanceConfiguration(), new TestPriceProvider(), w.Object);
            mockAlfen.CallBase = true;
            mockAlfen.Setup((x) => x.ReadProductInformation()).Returns(new EMS.Library.ProductInformation());
            mockAlfen.Setup((x) => x.ReadStationStatus()).Returns(new EMS.Library.StationStatus());
            mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(new AlfenNG9xx.Model.SocketMeasurement());

            using var cts = new CancellationTokenSource();
            await mockAlfen.Object.StartAsync(cts.Token).ConfigureAwait(false);

            Thread.Sleep(800);

            mockAlfen.Object.ExecuteTask.Should().NotBeNull();

            Assert.NotNull(mockAlfen.Object.ExecuteTask); // get rid of warning ;-)
            mockAlfen.Object.ExecuteTask.IsCompleted.Should().BeFalse();

            await mockAlfen.Object.StopAsync(cts.Token).ConfigureAwait(false);

            mockAlfen.Object.ExecuteTask.Should().BeNull();

            mockAlfen.Object.Dispose();
            mockAlfen.Object.Disposed.Should().BeTrue();

        }

        [Fact]
        public void HandleWorkRaisesEvents()
        {
            var w = new Mock<IWatchdog>();
            var mockAlfen = new Mock<AlfenNG9xx.AlfenBase>(new InstanceConfiguration(), new TestPriceProvider(), w.Object);
            mockAlfen.CallBase = true;
            mockAlfen.Setup((x) => x.ReadProductInformation()).Returns(new EMS.Library.ProductInformation());
            mockAlfen.Setup((x) => x.ReadStationStatus()).Returns(new EMS.Library.StationStatus());
            mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(new AlfenNG9xx.Model.SocketMeasurement());

#pragma warning disable CA1508
            int chargingStatusUpdateRaised = 0;
            EMS.Library.ChargingStatusUpdateEventArgs? lastChargingStatusUpdateEventArgs = null;
            mockAlfen.Object.ChargingStatusUpdate += (object? sender, EMS.Library.ChargingStatusUpdateEventArgs e) =>
            {
                chargingStatusUpdateRaised++;
                lastChargingStatusUpdateEventArgs = e;
            };

            int chargingStateUpdateRaised = 0;
            EMS.Library.ChargingStateEventArgs? lastChargingStateEventArgs = null;
            mockAlfen.Object.ChargingStateUpdate += (object? sender, EMS.Library.ChargingStateEventArgs e) =>
            {
                chargingStateUpdateRaised++;
                lastChargingStateEventArgs = e;
            };

            // FIRST
            mockAlfen.Object.HandleWork();
            chargingStatusUpdateRaised.Should().Be(1);
            lastChargingStatusUpdateEventArgs?.Status.Measurement.Mode3State.Should().Be(EMS.Library.Adapter.EVSE.Mode3State.A);

            chargingStateUpdateRaised.Should().Be(1);
            lastChargingStateEventArgs.Should().NotBeNull();

            mockAlfen.Object.Dispose();
            mockAlfen.Object.Disposed.Should().BeTrue();
#pragma warning restore CA1508
        }

        [Fact]
        public void HandleWorkRaisesChargingStateEventWhenStateWasChanged()
        {
            var socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement();

            var w = new Mock<IWatchdog>();
            var mockAlfen = new Mock<AlfenNG9xx.AlfenBase>(new InstanceConfiguration(), new TestPriceProvider(), w.Object);
            mockAlfen.CallBase = true;
            mockAlfen.Setup((x) => x.ReadProductInformation()).Returns(new EMS.Library.ProductInformation());
            mockAlfen.Setup((x) => x.ReadStationStatus()).Returns(new EMS.Library.StationStatus());
            mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);

#pragma warning disable CA1508
            int chargingStatusUpdateRaised = 0;
            EMS.Library.ChargingStatusUpdateEventArgs? lastChargingStatusUpdateEventArgs = null;
            mockAlfen.Object.ChargingStatusUpdate += (object? sender, EMS.Library.ChargingStatusUpdateEventArgs e) =>
            {
                chargingStatusUpdateRaised++;
                lastChargingStatusUpdateEventArgs = e;
            };

            int chargingStateUpdateRaised = 0;
            EMS.Library.ChargingStateEventArgs? lastChargingStateEventArgs = null;
            mockAlfen.Object.ChargingStateUpdate += (object? sender, EMS.Library.ChargingStateEventArgs e) =>
            {
                chargingStateUpdateRaised++;
                lastChargingStateEventArgs = e;
            };

            // FIRST
            mockAlfen.Object.HandleWork();
            chargingStatusUpdateRaised.Should().Be(1);
            lastChargingStatusUpdateEventArgs?.Status.Measurement.Mode3State.Should().Be(EMS.Library.Adapter.EVSE.Mode3State.A);

            chargingStateUpdateRaised.Should().Be(1);
            lastChargingStateEventArgs.Should().NotBeNull();

            // SECOND, but no charging state change
            mockAlfen.Object.HandleWork();
            chargingStatusUpdateRaised.Should().Be(2);
            chargingStateUpdateRaised.Should().Be(1);

            // THIRD, state is changing
            // creating new object... should create immutable
            var socketMeasurement2 = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.B1 };
            mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement2);
            mockAlfen.Object.HandleWork();
            chargingStatusUpdateRaised.Should().Be(3);
            chargingStateUpdateRaised.Should().Be(2);

            mockAlfen.Object.Dispose();
            mockAlfen.Object.Disposed.Should().BeTrue();
#pragma warning restore CA1508
        }

        [Fact(DisplayName = "Determine no current properly")]
        public void DetermineNoCurrent()
        {
            (var max, var phases) = AlfenBase.DetermineMaxCurrent(-1, -2, -3);
            Assert.Equal(-1, max, 0.1);
            Assert.Equal(0, phases);
        }

        [Fact(DisplayName = "Determine max current one phase (supply one)")]
        public void DetermineMaxCurrentOnePhase1()
        {
            (var max, var phases) = AlfenBase.DetermineMaxCurrent(14.1, 0, 0);
            Assert.Equal(14.1, max, 0.1);
            Assert.Equal(1, phases);
        }

        [Fact(DisplayName = "Determine max current one phase (suply two)")]
        public void DetermineMaxCurrentOnePhase2()
        {
            (var max, var phases) = AlfenBase.DetermineMaxCurrent(14.2, 10.0, 0);
            Assert.Equal(14.2, max, 0.1);
            Assert.Equal(1, phases);
        }

        [Fact(DisplayName = "Determine max current three phases (second phase leading)")]
        public void DetermineMaxCurrentThreePhase1()
        {
            (var max, var phases) = AlfenBase.DetermineMaxCurrent(15.2, 14.2, 15.1);
            Assert.Equal(14.2, max, 0.1);
            Assert.Equal(3, phases);
        }

        [Fact(DisplayName = "Determine max current three phases (third phase current)")]
        public void DetermineMaxCurrentThreePhase2()
        {
            (var max, var phases) = AlfenBase.DetermineMaxCurrent(15.3, 15.4, 14.3);
            Assert.Equal(14.3, max, 0.1);
            Assert.Equal(3, phases);
        }

        [Fact]
        void PlatformTypeToModelHandlesUnknownModelProperly()
        {
            AlfenBase.PlatformTypeToModel("XyZ").Should().Be("Unknown platform type XyZ");
        }

        [Theory]
        [InlineData("NG900", "Alfen Eve Single S-line")]
        [InlineData("NG910", "Alfen Eve Single Pro-line")]
        [InlineData("NG920", "Alfen Eve Double Pro-line / Eve Double PG / Twin 4XL")]
        void PlatformTypeToModelHandlesTranslationProperly2(string type, string model)
        {
            AlfenBase.PlatformTypeToModel(type).Should().Be(model);
        }
    }
}

