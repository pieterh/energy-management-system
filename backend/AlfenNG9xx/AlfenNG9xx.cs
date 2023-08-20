using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Configuration;
using EMS.Library.Core;
using EMS.Library.Exceptions;
using EMS.Library.TestableDateTime;

using AlfenNG9xx.Modbus;
using AlfenNG9xx.Model;
using System.Net.Sockets;

namespace AlfenNG9xx
{
    public class Alfen : AlfenBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly object _modbusMasterLock = new();

        [SuppressMessage("Code Analysis", "CA2213")]
        private IModbusMaster? _modbusMaster;
        internal IModbusMaster? ModbusMaster { get => _modbusMaster; private set => _modbusMaster = value; }

        private readonly string _alfenIp;
        private readonly int _alfenPort;

        public static void ConfigureServices(IServiceCollection services, AdapterInstance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            BackgroundServiceHelper.CreateAndStart<IChargePoint, Alfen>(services, instance.Config);
        }

        public Alfen(InstanceConfiguration config, IPriceProvider priceProvider, IWatchdog watchdog) : base(config, priceProvider, watchdog)
        {
            _alfenIp = config.Host;
            _alfenPort = config.Port;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                DisposeModbusMaster();
            }
        }

        protected void DisposeModbusMaster()
        {
            lock (_modbusMasterLock)
            {
                _modbusMaster?.Dispose();
                _modbusMaster = null;
            }
        }

        public override ProductIdentification ReadProductInformation()
        {
            var pi = ReadHoldingRegisters(200, 100, 79);

            Logger.Trace(HexDumper.ConvertToHexDump(pi));

            var result = new ProductIdentification();
            result.Name = Converters.ConvertRegistersToString(pi, 0, 17);
            result.Manufacturer = Converters.ConvertRegistersToString(pi, 17, 5);
            result.TableVersion = Converters.ConvertRegistersShort(pi, 22);
            result.FirmwareVersion = Converters.ConvertRegistersToString(pi, 23, 11);
            result.PlatformType = Converters.ConvertRegistersToString(pi, 40, 17);
            result.StationSerial = Converters.ConvertRegistersToString(pi, 57, 11);

            result.DateTimeUtc = new DateTime(
                    Converters.ConvertRegistersShort(pi, 68),
                    Converters.ConvertRegistersShort(pi, 69),
                    Converters.ConvertRegistersShort(pi, 70),
                    Converters.ConvertRegistersShort(pi, 71),
                    Converters.ConvertRegistersShort(pi, 72),
                    Converters.ConvertRegistersShort(pi, 73),
                    DateTimeKind.Utc
                ).ToUniversalTime();
            result.StationTimezone = Converters.ConvertRegistersShort(pi, 78);
            result.DateTimeLocal = new DateTime(result.DateTimeUtc.AddMinutes(result.StationTimezone).Ticks, DateTimeKind.Local);
            result.Uptime = (long)Converters.ConvertRegistersLong(pi, 74);
            result.UpSinceUtc = DateTime.UtcNow.AddMilliseconds(0 - (double)result.Uptime);
            result.Model = PlatformTypeToModel(result.PlatformType);

            return result;
        }

        public override AlfenNG9xx.Model.StationStatus ReadStationStatus()
        {
            var ss = new AlfenNG9xx.Model.StationStatus();
            var stationStatus = ReadHoldingRegisters(200, 1100, 6);
            Logger.Trace(HexDumper.ConvertToHexDump(stationStatus));

            ss.ActiveMaxCurrent = Converters.ConvertRegistersFloat(stationStatus, 0);
            ss.Temperature = Converters.ConvertRegistersFloat(stationStatus, 2);
            ss.OCCPState = Converters.ConvertRegistersShort(stationStatus, 4) == 0 ? OccpState.Disconnected : OccpState.Connected;
            ss.NrOfSockets = Converters.ConvertRegistersShort(stationStatus, 5);
            return ss;
        }

        public override SocketMeasurement ReadSocketMeasurement(byte socket)
        {
            var sm = new SocketMeasurement();
            var sm_part1 = ReadHoldingRegisters(socket, 300, 125);       // 126?
            var sm_part2 = ReadHoldingRegisters(socket, 1200, 16);
            Logger.Trace("---");
            Logger.Trace(HexDumper.ConvertToHexDump(sm_part1));
            Logger.Trace(HexDumper.ConvertToHexDump(sm_part2));

            sm.MeterState = Converters.ConvertRegistersShort(sm_part1, 0);
            sm.MeterTimestamp = Converters.ConvertRegistersLong(sm_part1, 1);

            sm.MeterType = SocketMeasurement.ParseMeterType(Converters.ConvertRegistersShort(sm_part1, 5));
            sm.VoltageL1 = Converters.ConvertRegistersFloat(sm_part1, 6);
            sm.VoltageL2 = Converters.ConvertRegistersFloat(sm_part1, 8);
            sm.VoltageL3 = Converters.ConvertRegistersFloat(sm_part1, 10);
            sm.Voltage = sm.VoltageL1;
            /* Voltage L1-L2*/
            /* Voltage L2-L3*/
            /* Voltage L3-L1*/

            /* Current N is null */
            sm.CurrentL1 = Converters.ConvertRegistersFloat(sm_part1, 20);
            sm.CurrentL2 = Converters.ConvertRegistersFloat(sm_part1, 22);
            sm.CurrentL3 = Converters.ConvertRegistersFloat(sm_part1, 24);

            // sum is null, calculate it
            sm.CurrentSum = sm.CurrentL1.Value + sm.CurrentL2.Value + sm.CurrentL3.Value;


            sm.RealPowerL1 = Converters.ConvertRegistersFloat(sm_part1, 38);
            sm.RealPowerL2 = Converters.ConvertRegistersFloat(sm_part1, 40);
            sm.RealPowerL3 = Converters.ConvertRegistersFloat(sm_part1, 42);
            sm.RealPowerSum = Converters.ConvertRegistersFloat(sm_part1, 45);

            /* power factor l1, l2 and l3 are null */
            /* power factor sum 1 */
            /* frequency */
            /* real power l1, l2, l3 and sum have values */
            /* apparent power and reactive power are null */
            sm.RealEnergyDeliveredL1 = Converters.ConvertRegistersDouble(sm_part1, 62);
            sm.RealEnergyDeliveredL2 = Converters.ConvertRegistersDouble(sm_part1, 66);
            sm.RealEnergyDeliveredL3 = Converters.ConvertRegistersDouble(sm_part1, 70);
            sm.RealEnergyDeliveredSum = Converters.ConvertRegistersDouble(sm_part1, 74);
            /* rest of part1 is null */

            sm.Availability = Converters.ConvertRegistersShort(sm_part2, 0) == 1;

            sm.Mode3State = SocketMeasurement.ParseMode3State(Converters.ConvertRegistersToString(sm_part2, 1, 5));
            sm.LastChargingStateChanged = (LastSocketMeasurement == null || LastSocketMeasurement.Mode3State != sm.Mode3State) ? DateTimeProvider.Now : LastSocketMeasurement.LastChargingStateChanged;

            sm.AppliedMaxCurrent = Converters.ConvertRegistersFloat(sm_part2, 6);
            sm.MaxCurrentValidTime = Converters.ConvertRegistersUInt32(sm_part2, 8);
            sm.MaxCurrent = Converters.ConvertRegistersFloat(sm_part2, 10);
            sm.ActiveLBSafeCurrent = Converters.ConvertRegistersFloat(sm_part2, 12);
            sm.SetPointAccountedFor = Converters.ConvertRegistersShort(sm_part2, 14) == 1;
            sm.Phases = SocketMeasurement.ParsePhases(Converters.ConvertRegistersShort(sm_part2, 15));

            return sm;
        }

        [SuppressMessage("CA", "CA1031", Justification = "The HandleException will perform proper handling")]
        protected virtual ushort[] ReadHoldingRegisters(byte slave, ushort address, ushort count)
        {
            lock (_modbusMasterLock)
            {
                try
                {
                    var modbusMaster = EnsureModbusMasterInitialized();
                    if (modbusMaster == null)
                        return Array.Empty<ushort>();

                    return modbusMaster.ReadHoldingRegisters(slave, address, count);
                }
                catch (Exception e)
                {
                    HandleException(e);                    
                }
            }
            return Array.Empty<ushort>();  // Added this to ensure all code paths return a value
        }

        [SuppressMessage("CA", "CA1031", Justification = "The HandleException will perform proper handling")]
        protected override void PerformUpdateMaxCurrent(double maxCurrent, ushort phases)
        {
            lock (_modbusMasterLock)
            {
                try
                {
                    var modbusMaster = EnsureModbusMasterInitialized();
                    if (modbusMaster == null)
                        return;

                    Logger.Trace($"PerformUpdateMaxCurrent {maxCurrent}, {phases}");
                    modbusMaster.WriteRegisters(1, 1210, Converters.ConvertFloatToRegisters((float)maxCurrent));
                    modbusMaster.WriteRegister(1, 1215, phases);
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
            }
        }

        private IModbusMaster? EnsureModbusMasterInitialized()
        {
            if (_modbusMaster != null) return _modbusMaster;

            _modbusMaster = ModbusMasterFactory();

            if (_modbusMaster == null)
            {
                Logger.Error($"Operation failed, no connection");                
            }

            return _modbusMaster;
        }

        private void HandleException(Exception e)
        {
            string exceptionMessage = e.Message;

            if (e is System.IO.IOException)
            {
                Logger.Error("Received an IOException, we try later again", e);
                exceptionMessage = "IOException";
            }
            else if (e is InvalidOperationException)
            {
                Logger.Error($"Received an InvalidOperationException, we try later again", e);
            }
            else if (e is SocketException se)
            {
                Logger.Error($"SocketException {se.Message}, we try later again");
            }
            else if (e.Message.StartsWith("Partial packet exception", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Error("Partial Modbus packaged received, we try later again");
                exceptionMessage = "Partial Modbus packaged received.";
            }
            else if (e.Message.StartsWith("Timeout connecting", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Error("{Message}", e.Message);
                exceptionMessage = "Modbus connection timeout.";
            }

            Logger.Error("Disposing connection");
            DisposeModbusMaster();

            throw new CommunicationException(exceptionMessage, e);
        }
        
        internal virtual IModbusMaster ModbusMasterFactory()
        {
            return AlfenNG9xx.Modbus.ModbusMaster.TCP(_alfenIp, _alfenPort, 2500);
        }
    }
}
