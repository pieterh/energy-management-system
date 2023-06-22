using System.Runtime.Intrinsics.X86;
using AlfenNG9xx;
using AlfenNG9xx.Tests;
using EMS.Library;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Configuration;
using EMS.Library.TestableDateTime;
using Moq;
using Moq.Protected;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        /// <summary>
        /// Following example is charge tesla, with disconnect on button and one tariff change
        /// 2023-06-20 12:43:35.9206| 19| INFO|chargingstate|Mode 3 state A, "Standby", session ended false, energy delivered NULL
        /// 2023-06-20 12:43:38.3803| 16| INFO|chargingstate|Mode 3 state B1, "Vehicle detected", session ended false, energy delivered NULL
        /// 2023-06-20 12:43:39.1584| 19| INFO|chargingstate|Mode 3 state B2, "Vehicle detected (PWM signal applied)", session ended false, energy delivered NULL
        /// 2023-06-20 12:43:40.0587| 19| INFO|chargingstate|Mode 3 state C2, "Charging", session ended false, energy delivered NULL
        /// 2023-06-20 13:43:41.7192| 24| INFO|chargingstate|Mode 3 state B2, "Vehicle detected (PWM signal applied)", session ended false, energy delivered NULL
        /// 2023-06-20 14:58:11.4134| 11| INFO|chargingstate|Mode 3 state A, "Standby", session ended true, energy delivered 9920
        /// 2023-06-20 14:58:21.6805| 11| INFO|chargingstate|Mode 3 state B1, "Vehicle detected", session ended false, energy delivered NULL
        /// 2023-06-20 14:58:22.4576| 11| INFO|chargingstate|Mode 3 state E, "No Power", session ended true, energy delivered 0
        /// </summary>
        [Fact]
        public void HandleWorkSimpleChargeUsingTariff()
        {
            var socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement();

            // https://mijn.easyenergy.com/nl/api/tariff/getapxtariffs?startTimestamp=2023-06-19T09%3A00%3A00.0000000Z&endTimestamp=2023-06-21T22%3A00%3A00.0000000Z
            var priceProvider = new TestPriceProvider();
            Tariff[] tariffs = new Tariff[] {
                new Tariff(new DateTime(2023, 06, 20,  9, 0, 0, DateTimeKind.Utc), 0.1176604m, 0.09724m),
                new Tariff(new DateTime(2023, 06, 20, 10, 0, 0, DateTimeKind.Utc), 0.1128930m, 0.09330m),
                new Tariff(new DateTime(2023, 06, 20, 11, 0, 0, DateTimeKind.Utc), 0.1089121m, 0.09001m),
                new Tariff(new DateTime(2023, 06, 20, 12, 0, 0, DateTimeKind.Utc), 0.1069035m, 0.08835m),
                new Tariff(new DateTime(2023, 06, 20, 13, 0, 0, DateTimeKind.Utc), 0.1139941m, 0.09421m),
                new Tariff(new DateTime(2023, 06, 20, 14, 0, 0, DateTimeKind.Utc), 0.1187736m, 0.09816m),
                new Tariff(new DateTime(2023, 06, 20, 15, 0, 0, DateTimeKind.Utc), 0.1331968m, 0.11008m),
                new Tariff(new DateTime(2023, 06, 20, 16, 0, 0, DateTimeKind.Utc), 0.1657700m, 0.13700m),
                new Tariff(new DateTime(2023, 06, 20, 17, 0, 0, DateTimeKind.Utc), 0.2057847m, 0.17007m)
            };

            priceProvider.SetTariffs(tariffs);

            var w = new Mock<IWatchdog>();
            var mockAlfen = new Mock<AlfenNG9xx.AlfenBase>(new InstanceConfiguration(), priceProvider, w.Object);
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


            using (new DateTimeProviderContext(new DateTimeOffset(2023, 06, 20, 12, 43, 35, 920, 600, new TimeSpan(2, 0, 0))))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.A };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(1);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeFalse();
            }

            var start = new DateTimeOffset(2023, 06, 20, 12, 43, 38, 380, 300, new TimeSpan(2, 0, 0));
            using (new DateTimeProviderContext(start))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.B1 };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(2);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeFalse();
            }

            using (new DateTimeProviderContext(new DateTimeOffset(2023, 06, 20, 12, 43, 39, 158, 400, new TimeSpan(2, 0, 0))))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.B2 };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(3);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeFalse();
            }

            using (new DateTimeProviderContext(new DateTimeOffset(2023, 06, 20, 12, 43, 40, 58, 700, new TimeSpan(2, 0, 0))))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.C2, RealEnergyDeliveredSum = 0 };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(4);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeTrue();
            }

            // still charging, need to update due to tarrif change... 16,3 minutes of charging
            using (new DateTimeProviderContext(new DateTimeOffset(2023, 06, 20, 13, 0, 0, 58, 0, new TimeSpan(2, 0, 0))))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.C2, RealEnergyDeliveredSum = 2695 };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(4);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeTrue();
            }

            using (new DateTimeProviderContext(new DateTimeOffset(2023, 06, 20, 14, 58, 11, 719, 200, new TimeSpan(2, 0, 0))))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.B2, RealEnergyDeliveredSum = 9920 };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(5);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeFalse();
            }

            var end = new DateTimeOffset(2023, 06, 20, 14, 58, 11, 413, 400, new TimeSpan(2, 0, 0));
            using (new DateTimeProviderContext(end))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.A, RealEnergyDeliveredSum = 9920 };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(6);

                Assert.NotNull(lastChargingStateEventArgs);
                lastChargingStateEventArgs.SessionEnded.Should().BeTrue();

                lastChargingStateEventArgs.Status.IsCharging.Should().BeFalse();
                lastChargingStateEventArgs.Start.Should().Be(start);
                lastChargingStateEventArgs.End.Should().Be(end);
                lastChargingStateEventArgs.EnergyDelivered.Should().Be(9920);
                //lastChargingStateEventArgs.Cost.Should().BeApproximately(1.09m, 0.005m);
                lastChargingStateEventArgs.Costs.Should().HaveCount(2);

                lastChargingStateEventArgs.Costs[0].Timestamp.Should().BeCloseTo(new DateTimeOffset(2023, 06, 20, 12, 43, 40, 58, 700, new TimeSpan(2, 0, 0)), new TimeSpan(0, 0, 1));
                lastChargingStateEventArgs.Costs[0].Energy.Should().Be(2695);
                Assert.NotNull(lastChargingStateEventArgs.Costs[0].Tariff);
                lastChargingStateEventArgs.Costs[0].Tariff?.Timestamp.Should().Be(new DateTime(2023, 06, 20, 10, 0, 0, DateTimeKind.Utc));

                lastChargingStateEventArgs.Costs[1].Timestamp.Should().BeCloseTo(new DateTimeOffset(2023, 06, 20, 13, 00, 00, 00, 000, new TimeSpan(2, 0, 0)), new TimeSpan(0, 0, 1));
                lastChargingStateEventArgs.Costs[1].Energy.Should().Be(7225);
                Assert.NotNull(lastChargingStateEventArgs.Costs[1].Tariff);
                lastChargingStateEventArgs.Costs[1].Tariff?.Timestamp.Should().Be(new DateTime(2023, 06, 20, 11, 0, 0, DateTimeKind.Utc));
            }

            ///////
            ///////
            ///////
            using (new DateTimeProviderContext(new DateTimeOffset(2023, 06, 20, 14, 58, 22, 680, 500, new TimeSpan(2, 0, 0))))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.B1 };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(7);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeFalse();
            }

            using (new DateTimeProviderContext(new DateTimeOffset(2023, 06, 20, 14, 58, 21, 457, 600, new TimeSpan(2, 0, 0))))
            {
                socketMeasurement = new AlfenNG9xx.Model.SocketMeasurement() { Mode3State = EMS.Library.Adapter.EVSE.Mode3State.E };
                mockAlfen.Setup((x) => x.ReadSocketMeasurement(It.IsAny<byte>())).Returns(socketMeasurement);
                mockAlfen.Object.HandleWork();
                chargingStateUpdateRaised.Should().Be(8);
                lastChargingStateEventArgs?.Status.IsCharging.Should().BeFalse();
            }

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

    internal static class DT
    {
        internal static TimeZoneInfo tziCEST = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        internal static TimeZoneInfo tziWEST = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        internal static DateTime WEST(this DateTime? dt)
        {
            return TimeZoneInfo.ConvertTime(dt ?? DateTimeProvider.Now, tziWEST);
        }

        internal static DateTimeOffset WEST(this DateTimeOffset dto)
        {
            return TimeZoneInfo.ConvertTime(dto, tziWEST);
        }

        internal static DateTime CEST(this DateTime dt)
        {
            return TimeZoneInfo.ConvertTime(dt, tziCEST);
        }

        internal static DateTimeOffset CEST(this DateTimeOffset dto)
        {
            return TimeZoneInfo.ConvertTime(dto, tziCEST);
        }
    }
}

