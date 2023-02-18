using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using Moq.Protected;
using AlfenNG9xx.Model;
using EMS.Library.Configuration;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using System.Threading.Tasks;
using System.Threading;
using EMS.Library.TestableDateTime;

namespace AlfenNG9xx.Tests
{
    public class AlfenTests
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        [Fact]
        public void DisposesProperly()
        {
            //var mock = new Mock<AlfenNG9xx.Alfen>(new Config() { Host = "192.168.1.9", Port = 502, Type = "LAN" });
            //mock.Protected()
            //    .Setup<ushort[]>("ReadHoldingRegisters", ItExpr.IsAny<byte>(), ItExpr.IsAny<ushort>(), ItExpr.IsAny<ushort>())
            //    .Returns<byte, ushort, ushort>((slave, address, count) => { return piRegisters; });
            //mock.Object.Dispose();

            var alf = new AlfenNG9xx.Alfen(new Config() { Host = "192.168.1.9", Port = 502, Type = "LAN" }, new TestPriceProvider());
            alf.Dispose();
        }

        [Fact]
        public void ReadProductIdentification()
        {
            byte[] piBytes = {
                0x4c, 0x41, 0x2d, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x33 ,0x00, 0x30, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x6c, 0x41, 0x65, 0x66, 0x20, 0x6e, 0x56, 0x4e, 0x00, 0x00, 0x01, 0x00, 0x2e, 0x34,
                0x30, 0x30, 0x30, 0x2e, 0x33, 0x2d, 0x39, 0x39, 0x00, 0x39, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x47, 0x4e, 0x31, 0x39, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x63, 0x61, 0x30, 0x65, 0x30, 0x30, 0x38, 0x35, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe4, 0x07, 0x0c, 0x00, 0x17, 0x00, 0x14, 0x00,
                0x31, 0x00, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0xdc, 0x06, 0x7e, 0xf7, 0x3c, 0x00
                };
            ushort[] piRegisters = new ushort[piBytes.Length / 2];
            Buffer.BlockCopy(piBytes, 0, piRegisters, 0, piBytes.Length);
            
            var mock = new Mock<AlfenNG9xx.Alfen>(new Config() { Host = "192.168.1.9", Port = 502, Type = "LAN" }, new TestPriceProvider());
            mock.Protected()
                .Setup<ushort[]>("ReadHoldingRegisters", ItExpr.IsAny<byte>(), ItExpr.IsAny<ushort>(), ItExpr.IsAny<ushort>())
                .Returns<byte, ushort, ushort>((slave, address, count) => { return piRegisters; });

            var pi = mock.Object.ReadProductIdentification();

            Assert.Equal("ALF-0000300", pi.Name);
            Assert.Equal("Alfen NV", pi.Manufacturer);
            Assert.Equal(1, pi.TableVersion);
            Assert.Equal("4.00.0-3999", pi.FirmwareVersion);
            Assert.Equal("NG910", pi.PlatformType);
            Assert.Equal("ace0005800", pi.StationSerial);
            Assert.Equal(60, pi.StationTimezone);
            Assert.Equal(115144574UL, pi.Uptime);
            Assert.Equal(DateTimeKind.Utc, pi.DateTimeUtc.Kind);
            Assert.Equal(DateTimeKind.Local, pi.DateTimeLocal.Kind);
            Assert.Equal(DateTimeKind.Utc, pi.UpSinceUtc.Kind);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.ReadStationStatus), MemberType = typeof(TestDataGenerator))]
        public void ReadStationStatus(byte[] piBytes, StationStatus expectedStationStatus)
        {

            ushort[] piRegisters = new ushort[piBytes.Length / 2];
            Buffer.BlockCopy(piBytes, 0, piRegisters, 0, piBytes.Length);

            var mock = new Mock<AlfenNG9xx.Alfen>(new Config() { Host = "192.168.1.9", Port = 502, Type = "LAN" }, new TestPriceProvider());
            mock.Protected()
                .Setup<ushort[]>("ReadHoldingRegisters", ItExpr.IsAny<byte>(), ItExpr.IsAny<ushort>(), ItExpr.IsAny<ushort>())
                .Returns<byte, ushort, ushort>((slave, address, count) => { return piRegisters; });

            var ss = mock.Object.ReadStationStatus();

            Assert.Equal(expectedStationStatus.ActiveMaxCurrent, ss.ActiveMaxCurrent);
            Assert.Equal(expectedStationStatus.Temperature, ss.Temperature);
            Assert.Equal(expectedStationStatus.OCCPState, ss.OCCPState);
            Assert.Equal(expectedStationStatus.NrOfSockets, ss.NrOfSockets);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.ReadSocketMeasurement), MemberType = typeof(TestDataGenerator))]
        public void ReadSocketMeasurement(byte[] piBytes_300, byte[] piBytes_1200, SocketMeasurement expectedSocketMeasurement)
        {
            ushort[] piRegisters_300 = ConvertBytesToRegisters(piBytes_300);
            ushort[] piRegisters_1200 = ConvertBytesToRegisters(piBytes_1200);

            var mock = new Mock<Alfen>(new Config() { Host = "192.168.1.9", Port = 502, Type = "LAN" }, new TestPriceProvider());
            mock.Protected()
                .Setup<ushort[]>("ReadHoldingRegisters", ItExpr.IsAny<byte>(), ItExpr.IsAny<ushort>(), ItExpr.IsAny<ushort>())
                .Returns<byte, ushort, ushort>((slave, address, count) =>
                {
                    return (address == 300) ? piRegisters_300 : piRegisters_1200;
                });

            var ss = mock.Object.ReadSocketMeasurement(1);

            Assert.Equal(expectedSocketMeasurement.MeterState, ss.MeterState);
            Assert.Equal(expectedSocketMeasurement.MeterTimestamp, ss.MeterTimestamp);
            Assert.Equal(expectedSocketMeasurement.MeterType, ss.MeterType);
            Assert.Equal(expectedSocketMeasurement.Availability, ss.Availability);
            Assert.Equal(expectedSocketMeasurement.Mode3State, ss.Mode3State);
            Assert.Equal(expectedSocketMeasurement.RealEnergyDeliveredL1, ss.RealEnergyDeliveredL1);
            Assert.Equal(expectedSocketMeasurement.RealEnergyDeliveredL2, ss.RealEnergyDeliveredL2);
            Assert.Equal(expectedSocketMeasurement.RealEnergyDeliveredL3, ss.RealEnergyDeliveredL3);
            Assert.Equal(expectedSocketMeasurement.RealEnergyDeliveredSum, ss.RealEnergyDeliveredSum);
            
            Assert.Equal(expectedSocketMeasurement.AppliedMaxCurrent, ss.AppliedMaxCurrent);
            Assert.Equal(expectedSocketMeasurement.MaxCurrentValidTime, ss.MaxCurrentValidTime);
            Assert.Equal(expectedSocketMeasurement.MaxCurrent, ss.MaxCurrent);
            Assert.Equal(expectedSocketMeasurement.ActiveLBSafeCurrent, ss.ActiveLBSafeCurrent);
            Assert.Equal(expectedSocketMeasurement.Phases, ss.Phases);
        }

        [Fact(DisplayName = "Determine no current properly")]
        public void DetermineNoCurrent()
        {
            (var max, var phases) = Alfen.DetermineMaxCurrent(-1, -2, -3);
            Assert.Equal(0, max, 0.1);
            Assert.Equal(0, phases);
        }

        [Fact(DisplayName = "Determine max current one phase (supply one)")]
        public void DetermineMaxCurrentOnePhase1()
        {
            (var max, var phases) = Alfen.DetermineMaxCurrent(14.1, 0, 0);
            Assert.Equal(14.1, max, 0.1);
            Assert.Equal(1, phases);
        }

        [Fact(DisplayName = "Determine max current one phase (suply two)")]
        public void DetermineMaxCurrentOnePhase2()
        {
            (var max, var phases) = Alfen.DetermineMaxCurrent(14.2, 10.0, 0);
            Assert.Equal(14.2, max, 0.1);
            Assert.Equal(1, phases);
        }

        [Fact(DisplayName = "Determine max current three phases (second phase leading)")]
        public void DetermineMaxCurrentThreePhase1()
        {
            (var max, var phases) = Alfen.DetermineMaxCurrent(15.2, 14.2, 15.1);
            Assert.Equal(14.2, max, 0.1);
            Assert.Equal(3, phases);
        }

        [Fact(DisplayName = "Determine max current three phases (third phase current)")]
        public void DetermineMaxCurrentThreePhase2()
        {
            (var max, var phases) = Alfen.DetermineMaxCurrent(15.3, 15.4, 14.3);
            Assert.Equal(14.3, max, 0.1);
            Assert.Equal(3, phases);
        }

        private static ushort[] ConvertBytesToRegisters(byte[] piBytes)
        {
            ushort[] registers = new ushort[piBytes.Length / 2];
            Buffer.BlockCopy(piBytes, 0, registers, 0, piBytes.Length);
            return registers;
        }
    }

    public static class TestDataGenerator
    {
        public static IEnumerable<object[]> ReadStationStatus()
        {
            yield return new object[]
            {
                new byte[] { 0x80, 0x41, 0x00, 0x00, 0x98, 0x41, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00 },
                new StationStatus {ActiveMaxCurrent = 16F, Temperature=19F, OCCPState = OccpState.Connected, NrOfSockets =(uint)1},
            };

            yield return new object[]
             {
                new byte[] { 0x80, 0x41, 0x00, 0x00, 0x98, 0x41, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00 },
                new StationStatus {ActiveMaxCurrent = 16F, Temperature=19F, OCCPState = OccpState.Disconnected, NrOfSockets =(uint)2},
             };
        }

        public static IEnumerable<object[]> ReadProductInformation()
        {
            yield return new object[] {
                new byte[] {
                        0x4c, 0x41, 0x2d, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x33 ,0x00, 0x30, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x6c, 0x41, 0x65, 0x66, 0x20, 0x6e, 0x56, 0x4e, 0x00, 0x00, 0x01, 0x00, 0x2e, 0x34,
                        0x30, 0x30, 0x30, 0x2e, 0x33, 0x2d, 0x39, 0x39, 0x00, 0x39, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x47, 0x4e, 0x31, 0x39, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x63, 0x61, 0x30, 0x65, 0x30, 0x30, 0x38, 0x35, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe4, 0x07, 0x0c, 0x00, 0x17, 0x00, 0x14, 0x00,
                        0x31, 0x00, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0xdc, 0x06, 0x7e, 0xf7, 0x3c, 0x00
                    }
            };
        }
        public static IEnumerable<object[]> ReadSocketMeasurement()
        {
            yield return new object[]
             {
                new byte[] {
                    0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x02, 0x00, 0x00, 0x6a, 0x43, 0x9a, 0x99,
                    0x6b, 0x43, 0x33, 0x33, 0x69, 0x43, 0x33, 0x33, 0xcb, 0x43, 0x9a, 0xd9, 0xca, 0x43, 0x67, 0xe6,
                    0xca, 0x43, 0xcd, 0x0c, 0xff, 0xff, 0xff, 0xff, 0x23, 0x3d, 0x0a, 0xd7, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0x0b, 0x3f, 0x96, 0x43, 0x48, 0x42, 0xeb, 0x51, 0x90, 0x40, 0xc2, 0xf5,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x40, 0xc2, 0xf5, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xf3, 0x40, 0xe0, 0xb1,
                    0x00, 0x00, 0x00, 0x00, 0xe1, 0x40, 0xc0, 0x0b, 0x00, 0x00, 0x00, 0x00, 0xe1, 0x40, 0xc0, 0x29,
                    0x00, 0x00, 0x00, 0x00, 0x02, 0x41, 0x50, 0x66, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
                },
                new byte[] {
                    0x01, 0x00, 0x00, 0x45, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x80, 0x41, 0x00, 0x00, 0x80, 0x41, 0x00, 0x00, 0x01, 0x00, 0x03, 0x00
                 },
                new SocketMeasurement {
                        MeterState = 3,
                        MeterTimestamp=592,
                        MeterType = MeterType.RTU,
                        Availability = true,
                        Mode3State = Mode3State.E,
                        RealEnergyDeliveredL1 = 80670,
                        RealEnergyDeliveredL2 =34910,
                        RealEnergyDeliveredL3 = 35150,
                        RealEnergyDeliveredSum = 150730,
                        AppliedMaxCurrent = 0f,
                        MaxCurrentValidTime = 0,
                        MaxCurrent = 16F,
                        ActiveLBSafeCurrent = 16f,
                        SetPointAccountedFor = true,
                        Phases = Phases.Three
                    }
                };

            yield return new object[]
             {
                new byte[] {
                    0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x02, 0x01, 0x02, 0x6a, 0x43, 0x9a, 0x99,
                    0x6b, 0x43, 0x33, 0x33, 0x69, 0x43, 0x33, 0x33, 0xcb, 0x43, 0x9a, 0xd9, 0xca, 0x43, 0x67, 0xe6,
                    0xca, 0x43, 0xcd, 0x0c, 0xff, 0xff, 0xff, 0xff, 0x23, 0x3d, 0x0a, 0xd7, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0x0b, 0x3f, 0x96, 0x43, 0x48, 0x42, 0xeb, 0x51, 0x90, 0x40, 0xc2, 0xf5,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x40, 0xc2, 0xf5, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xf3, 0x40, 0xe0, 0xb1,
                    0x00, 0x00, 0x00, 0x00, 0xe1, 0x40, 0xc0, 0x0b, 0x00, 0x00, 0x00, 0x00, 0xe1, 0x40, 0xc0, 0x29,
                    0x00, 0x00, 0x00, 0x00, 0x02, 0x41, 0x50, 0x66, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
                },
                new byte[] {
                    0x01, 0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x80, 0x41, 0x00, 0x00, 0x80, 0x41, 0x00, 0x00, 0x01, 0x00, 0x03, 0x00
                },
                new SocketMeasurement {
                        MeterState = 3,
                        MeterTimestamp=592,
                        MeterType = MeterType.UnknownType,
                        Availability = true,
                        Mode3State = Mode3State.UnknownState,
                        RealEnergyDeliveredL1 = 80670,
                        RealEnergyDeliveredL2 =34910,
                        RealEnergyDeliveredL3 = 35150,
                        RealEnergyDeliveredSum = 150730,
                        AppliedMaxCurrent = 0f,
                        MaxCurrentValidTime = 0,
                        MaxCurrent = 16F,
                        ActiveLBSafeCurrent = 16f,
                        SetPointAccountedFor = true,
                        Phases = Phases.Three
                    }
                };
        }
    }

    public class TestPriceProvider : IPriceProvider
    {
        public Task<Tariff[]> GetTariff(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public Tariff GetTariff()
        {
            var tariff = new Tariff(DateTimeProvider.Now, 0.23m, 0.08m);
            return tariff;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}