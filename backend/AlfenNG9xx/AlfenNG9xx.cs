using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpModbus;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Configuration;
using EMS.Library.TestableDateTime;

using AlfenNG9xx.Model;

namespace AlfenNG9xx
{
    public class Alfen : Microsoft.Extensions.Hosting.BackgroundService, IChargePoint
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed;
        internal bool isDisposed { get { return _disposed; } }

        private readonly IPriceProvider _priceProvider;

        private readonly object _modbusMasterLock = new();
        private ModbusMaster _modbusMaster;
        private readonly string _alfenIp;
        private readonly int _alfenPort;
        private readonly ChargingSession _chargingSession = new();

        public SocketMeasurementBase LastSocketMeasurement { get; private set; }
        public ChargeSessionInfoBase ChargeSessionInfo { get { return _chargingSession.ChargeSessionInfo; } }

        public event EventHandler<ChargingStatusUpdateEventArgs> ChargingStatusUpdate;
        public event EventHandler<ChargingStateEventArgs> ChargingStateUpdate;


        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services, Instance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            BackgroundServiceHelper.CreateAndStart<IChargePoint, Alfen>(services, instance.Config);
        }

        public Alfen(Config config, IPriceProvider priceProvider)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            dynamic cfg = config;
            Logger.Info($"Alfen({config.ToString().Replace(Environment.NewLine, " ", StringComparison.OrdinalIgnoreCase)})");
            _alfenIp = cfg.Host;
            _alfenPort = cfg.Port;

            _priceProvider = priceProvider;
        }

        public override void Dispose()
        {
            Grind(true);
            base.Dispose();
            GC.SuppressFinalize(this);  // Suppress finalization.
        }

        // calling this method grind to keep sonar happy
        // unfortunately the base class doesn't implement the dispose pattern
        // properly, so there is no method Dispose(bool disposing) to override...
        // https://github.com/dotnet/runtime/issues/34809
        internal virtual void Grind(bool disposing)
        {
            Logger.Trace($"Dispose({disposing}) _disposed {_disposed}");

            if (_disposed) return;

            if (disposing)
            {
                DisposeModbusMaster();
            }

            _disposed = true;
            Logger.Trace($"Dispose({disposing}) done => _disposed {_disposed}");
        }

        protected void DisposeModbusMaster()
        {
            lock (_modbusMasterLock)
            {
                _modbusMaster?.Dispose();
                _modbusMaster = null;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("Alfen NG9xx - Starting");
            try
            {
#if DEBUG
                ShowProductInformation();
                ShowStationStatus();
                ShowSocketMeasurement();
#endif

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        HandleWork();

                        await Task.Delay(1250, stoppingToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException tce)
                    {
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            Logger.Error("Exception: " + tce.Message);
                        }
                    }
                    catch (Exception e) when (e.Message.StartsWith("Partial exception packet", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Error("Partial Modbus packaged received, we try later again");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Unhandled, we try later again\n");
                        Logger.Error("Disposing connection");
                        DisposeModbusMaster();
                        await Task.Delay(2500, stoppingToken).ConfigureAwait(false);
                    }
                }

                Logger.Info("Alfen NG9xx - Canceled");
            }catch (Exception ex)
            {
                Logger.Error(ex, "Alfen NG9xx - Unhandled exception");
            }
        }

        private void HandleWork()
        {
            var sm = ReadSocketMeasurement(1);
            if (sm == null) return;

            var tariff = _priceProvider.GetTariff();
            _chargingSession.UpdateSession(sm, tariff);

            var chargingStateChanged = LastSocketMeasurement?.Mode3State != sm.Mode3State;

            LastSocketMeasurement = sm;
            ChargingStatusUpdate?.Invoke(this, new ChargingStatusUpdateEventArgs(sm));
            if (chargingStateChanged)
            {
                var sessionEnded = _chargingSession.ChargeSessionInfo.SessionEnded;
                var energyDelivered = _chargingSession.ChargeSessionInfo.EnergyDelivered;
                var cost = _chargingSession.ChargeSessionInfo.Cost;
                var costs = _chargingSession.ChargeSessionInfo.Costs;

                foreach (var c in _chargingSession.ChargeSessionInfo.Costs)
                {
                    Logger.Debug("Cost : {0}, {1}, {2}", c.Timestamp.ToLocalTime().ToString("O"), c.Energy, c.Tariff.TariffUsage);
                }
                ChargingStateUpdate?.Invoke(this, new ChargingStateEventArgs(sm, sessionEnded, energyDelivered, cost, costs));
            }
        }

#if DEBUG
        protected virtual void ShowProductInformation()
        {
            var pi = ReadProductIdentification();
            if (pi == null) return;
            Logger.Info(pi.ToPrintableString());
        }

        private void ShowStationStatus()
        {
            try
            {
                var status = ReadStationStatus();
                if (status == null) return;

                Logger.Info($"Station Active Max Current : {status.ActiveMaxCurrent}");
                Logger.Info($"Temperature                : {status.Temperature}");
                Logger.Info($"OCCP                       : {status.OCCPState}");
                Logger.Info($"Nr of sockets              : {status.NrOfSockets}");
            }
            catch (SystemException se)
            {
                Logger.Error(se, $"{se.Message}");
            }
        }

        private void ShowSocketMeasurement()
        {
            try
            {
                var sm = ReadSocketMeasurement(1);
                if (sm == null) return;

                Logger.Info($"Meter State                : {sm.MeterState}");
                Logger.Info($"Meter Timestamp            : {sm.MeterTimestamp}");
                Logger.Info($"Meter Type                 : {sm.MeterType}");

                Logger.Info($"Real Energy Delivered L1   : {sm.RealEnergyDeliveredL1}");
                Logger.Info($"Real Energy Delivered L2   : {sm.RealEnergyDeliveredL2}");
                Logger.Info($"Real Energy Delivered L3   : {sm.RealEnergyDeliveredL3}");
                Logger.Info($"Real Energy Delivered Sum  : {sm.RealEnergyDeliveredSum}");

                Logger.Info($"Availability               : {sm.Availability}");
                Logger.Info($"Mode 3 State               : {sm.Mode3State}");

                Logger.Info($"Actual Applied Max Current : {sm.AppliedMaxCurrent}");
                Logger.Info($"Max Current Valid Time     : {sm.MaxCurrentValidTime}");
                Logger.Info($"Max Current                : {sm.MaxCurrent}");
                Logger.Info($"Active Load Bl safe current: {sm.ActiveLBSafeCurrent}");
                Logger.Info($"Setpoint accounted for     : {sm.SetPointAccountedFor}");
                Logger.Info($"Using # phases             : {sm.Phases}");
            }
            catch (SystemException se)
            {
                Logger.Error(se, $"{se.Message}");
            }
        }
#endif
        public ProductIdentification ReadProductIdentification()
        {
            var result = new ProductIdentification();
            var pi = ReadHoldingRegisters(200, 100, 79);
            if (pi == null) return null;

            Logger.Trace(HexDumper.ConvertToHexDump(pi));

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

            switch (result.PlatformType)
            {
                case "NG900":
                    result.Model = "Alfen Eve Single S-line";
                    break;
                case "NG910":
                    result.Model = "Alfen Eve Single Pro-line";
                    break;
                case "NG920":
                    result.Model = "Alfen Eve Double Pro-line / Eve Double PG / Twin 4XL";
                    break;
                default:
                    result.Model = $"Unknown {result.PlatformType}";
                    break;
            }

            return result;
        }

        public AlfenNG9xx.Model.StationStatus ReadStationStatusInternal()
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

        public SocketMeasurement ReadSocketMeasurement(byte socket)
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

        protected virtual ushort[] ReadHoldingRegisters(byte slave, ushort address, ushort count)
        {
            lock (_modbusMasterLock)
            {
                if (_modbusMaster == null)
                    _modbusMaster = ModbusMaster.TCP(_alfenIp, _alfenPort, 2500);

                if (_modbusMaster == null)
                {
                    Logger.Error($"ReadHoldingRegisters() -> failed, no connection");
                    return Array.Empty<ushort>();
                }

                return _modbusMaster.ReadHoldingRegisters(slave, address, count);
            }
        }

        #region IChargePoint
        /// <summary>
        /// IChargePoint
        /// </summary>
        /// <returns></returns>
        public ProductInformation ReadProductInformation()
        {
            var pi = ReadProductIdentification();
            return pi;
        }

        /// <summary>
        /// IChargePoint
        /// </summary>
        /// <returns></returns>
        public EMS.Library.StationStatus ReadStationStatus()
        {
            var ss = ReadStationStatusInternal();
            return ss;
        }

        /// <summary>
        /// IChargePoint
        /// </summary>
        /// <param name="maxCurrent"></param>
        /// <param name="phases"></param>
        public void UpdateMaxCurrent(double maxCurrent, ushort phases)
        {
            Logger.Info($"UpdateMaxCurrent({maxCurrent}, {phases})");
            if (maxCurrent <= 0)
                return;

            lock (_modbusMasterLock)
            {
                if (_modbusMaster == null)
                    _modbusMaster = ModbusMaster.TCP(_alfenIp, _alfenPort, 2500);

                if (_modbusMaster == null)
                {
                    Logger.Info($"UpdateMaxCurrent({maxCurrent}, {phases}) -> failed, no connection");
                    return;
                }

                Logger.Info($"UpdateMaxCurrent {maxCurrent}, {phases}");
                _modbusMaster.WriteRegisters(1, 1210, Converters.ConvertFloatToRegisters((float)maxCurrent));
                _modbusMaster.WriteRegister(1, 1215, phases);
            }
        }

        /// <summary>
        /// IChargePoint
        /// </summary>
        /// <param name="maxL1"></param>
        /// <param name="maxL2"></param>
        /// <param name="maxL3"></param>
        public void UpdateMaxCurrent(double maxL1, double maxL2, double maxL3)
        {
            Logger.Info($"UpdateMaxCurrent({maxL1}, {maxL2}, {maxL3})");
            (double maxCurrent, ushort phases) = DetermineMaxCurrent(maxL1, maxL2, maxL3);
            if (maxCurrent <= 0)
                return;
            UpdateMaxCurrent(maxCurrent, phases);
        }

        #endregion
        public static (double, ushort) DetermineMaxCurrent(double maxL1, double maxL2, double maxL3)
        {
            Logger.Info($"UpdateMaxCurrent({maxL1}, {maxL2}, {maxL3})");

            if (maxL1 < 0f && maxL2 < 0f && maxL3 < 0f) return (-1, 0);
            ushort phases;
            float maxCurrent;
            if (maxL2 <= 0f || maxL3 <= 0f)
            {
                phases = 1;
                maxCurrent = (float)Math.Round(maxL1, 1, MidpointRounding.ToZero);
            }
            else
            {
                phases = 3;
                maxCurrent = (float)Math.Round(Math.Min(maxL1, Math.Min(maxL2, maxL3)), 1, MidpointRounding.ToZero);
            }

            return (maxCurrent, phases);
        }
    }
}
