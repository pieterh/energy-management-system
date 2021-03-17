using System;
using SharpModbus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AlfenNG9xx.Model;
using EMS.Library;

namespace AlfenNG9xx
{
    public class Alfen : BackgroundWorker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        ModbusMaster _modbusMaster = null;
        private readonly string _alfenIp ;
        private readonly int _alfenPort;

        private DateTime _lastMaxCurrentUpdate = DateTime.MinValue;

        public Alfen(JObject config)
        {
            dynamic cfg = config;
            Logger.Info($"Alfen({config.ToString().Replace(Environment.NewLine, " ")})");
            _alfenIp = cfg.host;
            _alfenPort  = cfg.port;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeModbusMaster();
        }
        private void DisposeModbusMaster()
        {
            if (_modbusMaster == null) return;

            try
            {
                _modbusMaster.Dispose();
            }
            finally
            {
                _modbusMaster = null;
            }

        }

        public override void Start()
        {
            Logger.Trace($"Starting");
            base.Start();
            ShowProductInformation();
        }

        protected override void DoBackgroundWork()
        {
            try
            {
                Logger.Info("Doing something usefull");
                ShowStationStatus();
                ShowSocketMeasurement();

                if (_lastMaxCurrentUpdate == DateTime.MinValue || (DateTime.Now - _lastMaxCurrentUpdate).Seconds > 20){
                    UpdateMaxCurrent(7.25f, Phases.One);
                }
            }
            catch (Exception e) when (e.Message.StartsWith("Partial exception packet"))
            {
                Logger.Error("Exception: " + e.Message);
                Logger.Error("Partial Modbus packaged received, we try later again");
            }catch (Exception e) when (e.Message.StartsWith("Broken pipe"))
            {
                Logger.Error("Exception: " + e.Message);
                Logger.Error("Broken pipe, we try later again");
                Logger.Error("Disposing connection");
                try
                {
                    _modbusMaster.Dispose();
                }
                finally
                {
                    _modbusMaster = null;
                }
            }catch (Exception e) 
            {
                Logger.Error("Exception: " + e.Message);
                Logger.Error("Unhandled, we try later again");
                Logger.Error("Disposing connection");
                try
                {
                    _modbusMaster.Dispose();
                }
                finally
                {
                    _modbusMaster = null;
                }
            }
        }
        protected virtual void ShowProductInformation()
        {
            //Modbus TCP over socket
            try
            {
                var pi = ReadProductIdentification();
                Console.WriteLine("Name                       : {0}", pi.Name);
                Console.WriteLine("Manufacterer               : {0}", pi.Manufacterer);
                Console.WriteLine("Table version              : {0}", pi.TableVersion);
                Console.WriteLine("Firmware version           : {0}", pi.FirmwareVersion);
                Console.WriteLine("Platform type              : {0}", pi.PlatformType);
                Console.WriteLine("Station serial             : {0}", pi.StationSerial);
                Console.WriteLine("Date Local                 : {0}", pi.DateTimeLocal.ToString("O"));
                Console.WriteLine("Date UTC                   : {0}", pi.DateTimeUtc.ToString("O"));
                Console.WriteLine("Uptime                     : {0}", pi.Uptime);
                Console.WriteLine("Up since                   : {0}", pi.UpSinceUtc.ToString("O"));
                Console.WriteLine("Timezone                   : {0}", pi.StationTimezone);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
        }

        private void ShowStationStatus()
        {
            var status = ReadStationStatus();

            Console.WriteLine($"Station Active Max Current : {status.ActiveMaxCurrent}");
            Console.WriteLine($"Temparature                : {status.Temparature}");
            Console.WriteLine($"OCCP                       : {status.OCCPState}");
            Console.WriteLine($"Nr of sockets              : {status.NrOfSockets}");

            //Logger.Trace(HexDumper.ConvertToHexDump(status.Temparature));
            //Logger.Trace(HexDumper.ConvertToHexDump((float)19.875));
        }

        private void ShowSocketMeasurement()
        {
            var sm = ReadSocketMeasurement(1);
            Console.WriteLine($"Meter State                : {sm.MeterState}");
            Console.WriteLine($"Meter Timestamp            : {sm.MeterTimestamp}");
            Console.WriteLine($"Meter Type                 : {sm.MeterType}");

            Console.WriteLine($"Real Energy Delivered L1   : {sm.RealEnergyDeliveredL1}");
            Console.WriteLine($"Real Energy Delivered L2   : {sm.RealEnergyDeliveredL2}");
            Console.WriteLine($"Real Energy Delivered L3   : {sm.RealEnergyDeliveredL3}");
            Console.WriteLine($"Real Energy Delivered Sum  : {sm.RealEnergyDeliveredSum}");

            Console.WriteLine($"Availability               : {sm.Availability}");
            Console.WriteLine($"Mode 3 State               : {sm.Mode3State}");

            Console.WriteLine($"Actual Applied Max Current : {sm.AppliedMaxCurrent}");
            Console.WriteLine($"Max Current Valid Time     : {sm.MaxCurrentValidTime}");
            Console.WriteLine($"Max Current                : {sm.MaxCurrent}");
            Console.WriteLine($"Active Load Bl safe current: {sm.ActiveLBSafeCurrent}");
            Console.WriteLine($"Setpoint accounted for     : {sm.SetPointAccountedFor}");
            Console.WriteLine($"Using # phases             : {sm.Phases}");
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
            sm.RealEnergyDeliveredL1 = Converters.ConvertRegistersDouble(sm_part1, 62);
            sm.RealEnergyDeliveredL2 = Converters.ConvertRegistersDouble(sm_part1, 66);
            sm.RealEnergyDeliveredL3 = Converters.ConvertRegistersDouble(sm_part1, 70);
            sm.RealEnergyDeliveredSum = Converters.ConvertRegistersDouble(sm_part1, 74);

            sm.Availability = Converters.ConvertRegistersShort(sm_part2, 0) == 1;

            sm.Mode3State = SocketMeasurement.ParseMode3State(Converters.ConvertRegistersToString(sm_part2, 1, 5));
            sm.AppliedMaxCurrent = Converters.ConvertRegistersFloat(sm_part2, 6);
            sm.MaxCurrentValidTime = Converters.ConvertRegistersUInt32(sm_part2, 8);
            sm.MaxCurrent = Converters.ConvertRegistersFloat(sm_part2, 10);
            sm.ActiveLBSafeCurrent= Converters.ConvertRegistersFloat(sm_part2, 12);
            sm.SetPointAccountedFor = Converters.ConvertRegistersShort(sm_part2, 14) == 1;
            sm.Phases = SocketMeasurement.ParsePhases(Converters.ConvertRegistersShort(sm_part2, 15));

            //Console.WriteLine(HexDumper.ConvertToHexDump(sm.MeterTimestamp));
            //Console.WriteLine(HexDumper.ConvertToHexDump((UInt64)1500));  
            //Console.WriteLine(HexDumper.ConvertToHexDump(sm.RealEnergyDeliveredSum));
            //Console.WriteLine(HexDumper.ConvertToHexDump((double)149290));                      
            return sm;
        }
        private void UpdateMaxCurrent(float maxCurrent, Phases phases){
            Logger.Info($"UpdateMaxCurrent({maxCurrent},{phases})", maxCurrent, phases);
            _modbusMaster.WriteRegisters(1, 1210, Converters.ConvertFloatToRegisters(maxCurrent));
            _modbusMaster.WriteRegister(1, 1215, (ushort)phases);
        }
        protected virtual ushort[] ReadHoldingRegisters(byte slave, ushort address, ushort count)
        {
            if (_modbusMaster == null)
                _modbusMaster = ModbusMaster.TCP(_alfenIp, _alfenPort, 2500);
            return _modbusMaster.ReadHoldingRegisters(slave, address, count);
        }
    }
}
