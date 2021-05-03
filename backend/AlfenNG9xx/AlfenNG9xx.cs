using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpModbus;
using EMS.Library;
using EMS.Library.Configuration;
using AlfenNG9xx.Model;
using EMS.Library.Adapter.EVSE;

namespace AlfenNG9xx
{
    public class Alfen : BackgroundService, IChargePoint
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed = false;

        ModbusMaster _modbusMaster = null;
        private readonly string _alfenIp;
        private readonly int _alfenPort;
        private readonly ChargingSession _chargingSession = new();

        public SocketMeasurementBase LastSocketMeasurement { get; private set; }
        public ChargeSessionInfoBase ChargeSessionInfo { get { return _chargingSession.ChargeSessionInfo; } }


        public event EventHandler<IChargePoint.StatusUpdateEventArgs> StatusUpdate;
        public event EventHandler<IChargePoint.ChargingStateEventArgs> ChargingStateUpdate;

        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services, Instance instance)
        {
            services.AddSingleton(typeof(IChargePoint), x =>
            {
                var s = ActivatorUtilities.CreateInstance(x, typeof(Alfen), instance.Config);
                Logger.Info($"Instance [{instance.Name}], created");
                return s;
            });
            services.AddSingleton<IHostedService>(x =>
            {
                var s = x.GetService(typeof(IChargePoint)) as IHostedService;
                return s;
            });
        }

        public Alfen(Config config)
        {
            dynamic cfg = config;
            Logger.Info($"Alfen({config.ToString().Replace(Environment.NewLine, " ")})");
            _alfenIp = cfg.Host;
            _alfenPort = cfg.Port;
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
        protected virtual void Grind(bool disposing)
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

        private void DisposeModbusMaster()
        {
            _modbusMaster?.Dispose();
            _modbusMaster = null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info($"Alfen Starting");

            ShowProductInformation();
            ShowStationStatus();
            ShowSocketMeasurement();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    HandleWork();

                    await Task.Delay(2500, stoppingToken);
                }
                catch (TaskCanceledException tce)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        Logger.Error("Exception: " + tce.Message);
                    }
                }
                catch (Exception e) when (e.Message.StartsWith("Partial exception packet"))
                {
                    Logger.Error("Partial Modbus packaged received, we try later again");
                }
                catch (Exception e)
                {
                    Logger.Error("Exception: " + e.Message);
                    Logger.Error("Unhandled, we try later again");
                    Logger.Error("Disposing connection");
                    DisposeModbusMaster();
                }
            }
            Logger.Info($"Canceled");
        }

        private void HandleWork()
        {
            var sm = ReadSocketMeasurement(1);
            _chargingSession.UpdateSession(sm);

            var chargingStateChanged = LastSocketMeasurement?.Mode3State != sm.Mode3State;
            
            LastSocketMeasurement = sm;
            StatusUpdate?.Invoke(this, new IChargePoint.StatusUpdateEventArgs(sm));
            if (chargingStateChanged)
            {
                var sessionEnded = _chargingSession.ChargeSessionInfo.SessionEnded;
                var energyDelivered = _chargingSession.ChargeSessionInfo.EnergyDelivered;

                ChargingStateUpdate?.Invoke(this, new IChargePoint.ChargingStateEventArgs(sm, sessionEnded, energyDelivered));
            }
        }

        protected virtual void ShowProductInformation()
        {
            //Modbus TCP over socket
            try
            {
                var pi = ReadProductIdentification();
                Logger.Info("Name                       : {0}", pi.Name);
                Logger.Info("Manufacterer               : {0}", pi.Manufacterer);
                Logger.Info("Table version              : {0}", pi.TableVersion);
                Logger.Info("Firmware version           : {0}", pi.FirmwareVersion);
                Logger.Info("Platform type              : {0}", pi.PlatformType);
                Logger.Info("Station serial             : {0}", pi.StationSerial);
                Logger.Info("Date Local                 : {0}", pi.DateTimeLocal.ToString("O"));
                Logger.Info("Date UTC                   : {0}", pi.DateTimeUtc.ToString("O"));
                Logger.Info("Uptime                     : {0}", pi.Uptime);
                Logger.Info("Up since                   : {0}", pi.UpSinceUtc.ToString("O"));
                Logger.Info("Timezone                   : {0}", pi.StationTimezone);
            }
            catch (Exception e)
            {
                Logger.Error(e, "{0}", e.ToString());
            }
        }

        private void ShowStationStatus()
        {
            var status = ReadStationStatus();

            Logger.Info($"Station Active Max Current : {status.ActiveMaxCurrent}");
            Logger.Info($"Temparature                : {status.Temparature}");
            Logger.Info($"OCCP                       : {status.OCCPState}");
            Logger.Info($"Nr of sockets              : {status.NrOfSockets}");
        }

        private void ShowSocketMeasurement()
        {
            var sm = ReadSocketMeasurement(1);
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

        public ProductIdentification ReadProductIdentification()
        {
            var result = new ProductIdentification();
            var pi = ReadHoldingRegisters(200, 100, 79);

            Logger.Trace(HexDumper.ConvertToHexDump(pi));

            result.Name = Converters.ConvertRegistersToString(pi, 0, 17);
            result.Manufacterer = Converters.ConvertRegistersToString(pi, 17, 5);
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
            result.Uptime = Converters.ConvertRegistersLong(pi, 74);
            result.UpSinceUtc = DateTime.UtcNow.AddMilliseconds(0 - (double)result.Uptime);

            return result;
        }
        public StationStatus ReadStationStatus()
        {
            var ss = new StationStatus();
            var stationStatus = ReadHoldingRegisters(200, 1100, 6);
            Logger.Trace(HexDumper.ConvertToHexDump(stationStatus));

            ss.ActiveMaxCurrent = Converters.ConvertRegistersFloat(stationStatus, 0);
            ss.Temparature = Converters.ConvertRegistersFloat(stationStatus, 2);
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
            sm.CurrentSum = sm.CurrentL1 + sm.CurrentL2 + sm.CurrentL3;


            sm.RealPowerL1 = Converters.ConvertRegistersFloat(sm_part1, 38);
            sm.RealPowerL2 = Converters.ConvertRegistersFloat(sm_part1, 40);
            sm.RealPowerL3 = Converters.ConvertRegistersFloat(sm_part1, 42);
            sm.RealPowerSum = Converters.ConvertRegistersFloat(sm_part1, 45);  /* */

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

            sm.LastChargingStateChanged = (LastSocketMeasurement == null || LastSocketMeasurement?.Mode3State != sm.Mode3State) ? DateTime.Now : LastSocketMeasurement.LastChargingStateChanged;

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
            if (_modbusMaster == null)
                _modbusMaster = ModbusMaster.TCP(_alfenIp, _alfenPort, 2500);

            lock (_modbusMaster)
            {
                return _modbusMaster.ReadHoldingRegisters(slave, address, count);
            }
        }


        public void UpdateMaxCurrent(double maxL1, double maxL2, double maxL3)
        {
            Logger.Info($"UpdateMaxCurrent({maxL1}, {maxL2}, {maxL3})");
            lock (_modbusMaster)
            {
                if (maxL1 < 0f && maxL2 < 0f && maxL3 < 0f) return;
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

                Logger.Info($"UpdateMaxCurrent {maxCurrent}, {phases}");
                _modbusMaster.WriteRegisters(1, 1210, Converters.ConvertFloatToRegisters(maxCurrent));
                _modbusMaster.WriteRegister(1, 1215, phases);
            }
        }
    }
}
