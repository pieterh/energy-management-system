using System;
using System.Threading;
using System.Threading.Tasks;

using SharpModbus;

using AlfenNG9xx.Model;

namespace AlfenNG9xx
{
    public class AlfenNG9xx
    {
        string alfenIp = "192.168.1.9";
        int alfenPort = 502;
        private Task backgroundTask = null;
        private CancellationTokenSource tokenSource = null;
        public AlfenNG9xx()
        {

        }

        public void Start()
        {
            ShowProductInformation();
            tokenSource = new CancellationTokenSource();
            backgroundTask = Task.Run(() => Run(), tokenSource.Token);
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }

        private void Run()
        {
            try
            {
                while (!StopRequested(2500))
                {
                    Console.WriteLine("Doing something usefull");
                    //ShowStationStatus();
                    ShowSocketMeasurement();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.GetType()}\n{ex.Message}, {ex.StackTrace}");
            }
        }

        bool StopRequested(int ms)
        {
            for (var i = 0; i < ms / 500; i++)
            {
                if (tokenSource.Token.IsCancellationRequested)
                    return true;
                Thread.Sleep(500);
            }
            return false;
        }

        public void ShowProductInformation()
        {
            //Modbus TCP over socket
            using (var master = ModbusMaster.TCP(alfenIp, alfenPort))
            {
                ShowProductInformation(master);
            }
        }
        public void ShowProductInformation(ModbusMaster master)
        {
            //Modbus TCP over socket
            try
            {
                var pi = ReadProductIdentification(master);
                Console.WriteLine("Name                       : {0}", pi.Name);
                Console.WriteLine("Manufacterer               : {0}", pi.Manufacterer);
                Console.WriteLine("Table version              : {0}", pi.TableVersion);
                Console.WriteLine("Station serial             : {0}", pi.StationSerial);
                Console.WriteLine("Date Local                 : {0}", pi.DateTimeLocal.ToString("O"));
                Console.WriteLine("Date UTC                   : {0}", pi.DateTimeUtc.ToString("O"));
                Console.WriteLine("Uptime                     : {0}", pi.Uptime);
                Console.WriteLine("Up since                   : {0} {1}", pi.UpSinceUtc.ToString(), pi.UpSinceUtc.Kind);
                Console.WriteLine("Timezone                   : {0}", pi.StationTimezone);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
        }

        private void ShowStationStatus()
        {
            using (var master = ModbusMaster.TCP(alfenIp, alfenPort))
            {
                ShowStationStatus(master);
            }
        }
        private void ShowStationStatus(ModbusMaster master)
        {
            var status = ReadStationStatus(master);

            Console.WriteLine($"Station Active Max Current : {status.ActiveMaxCurrent}");
            Console.WriteLine($"Temparature                : {status.Temparature}");
            Console.WriteLine($"OCCP                       : {status.OCCPState}");
            Console.WriteLine($"Nr of sockets              : {status.NrOfSockets}");


            //Console.WriteLine(HexDumper.ConvertToHexDump(status.Temparature));
            //Console.WriteLine(HexDumper.ConvertToHexDump((float)19.875));
        }



        private void ShowSocketMeasurement()
        {
            using (var master = ModbusMaster.TCP(alfenIp, alfenPort))
            {
                ShowSocketMeasurement(master);
            }
        }
        private void ShowSocketMeasurement(ModbusMaster master)
        {
            var sm = ReadSocketMeasurement(master, 1);
            Console.WriteLine($"Meter State                : {sm.MeterState}");
            Console.WriteLine($"Meter Timestamp            : {sm.MeterTimestamp}");
            Console.WriteLine($"Meter Type                 : {sm.MeterType}");
            Console.WriteLine($"Availability               : {sm.Availability}");
            Console.WriteLine($"Mode 3 State               : {sm.Mode3State}");

            Console.WriteLine($"Real Energy Delivered L1   : {sm.RealEnergyDeliveredL1}");
            Console.WriteLine($"Real Energy Delivered L2   : {sm.RealEnergyDeliveredL2}");
            Console.WriteLine($"Real Energy Delivered L3   : {sm.RealEnergyDeliveredL3}");
            Console.WriteLine($"Real Energy Delivered Sum  : {sm.RealEnergyDeliveredSum}");

            Console.WriteLine($"Max Current Valid Time     : {sm.MaxCurrentValidTime}");
            Console.WriteLine($"Max Current                : {sm.MaxCurrent}");            
        }

        public static ProductIdentification ReadProductIdentification(ModbusMaster master)
        {
            var result = new ProductIdentification();
            var pi = master.ReadHoldingRegisters(200, 100, 79);
            result.Name = Converters.ConvertRegistersToString(pi, 0, 17);
            result.Manufacterer = Converters.ConvertRegistersToString(pi, 17, 5);
            result.TableVersion = Converters.ConvertRegistersShort(pi, 22);
            result.StationSerial = Converters.ConvertRegistersToString(pi, 57, 11);

            result.DateTimeUtc = new DateTime(
                    Converters.ConvertRegistersShort(pi, 68),
                    Converters.ConvertRegistersShort(pi, 69),
                    Converters.ConvertRegistersShort(pi, 70),
                    Converters.ConvertRegistersShort(pi, 71),
                    Converters.ConvertRegistersShort(pi, 72),
                    Converters.ConvertRegistersShort(pi, 73),
                    DateTimeKind.Utc
                );
            result.StationTimezone = Converters.ConvertRegistersShort(pi, 78);
            result.DateTimeLocal = new DateTime(result.DateTimeUtc.AddMinutes(result.StationTimezone).Ticks, DateTimeKind.Local);
            result.Uptime = Converters.ConvertRegistersLong(pi, 74);
            result.UpSinceUtc = DateTime.UtcNow.AddMilliseconds(0 - (double)result.Uptime);

            return result;
        }
        private StationStatus ReadStationStatus(ModbusMaster master)
        {
            var ss = new StationStatus();
            var stationStatus = master.ReadHoldingRegisters(200, 1100, 6);
            Console.WriteLine(HexDumper.ConvertToHexDump(stationStatus));

            ss.ActiveMaxCurrent = Converters.ConvertRegistersFloat(stationStatus, 0);
            ss.Temparature = Converters.ConvertRegistersFloat(stationStatus, 2);
            ss.OCCPState = Converters.ConvertRegistersShort(stationStatus, 4) == 0 ? OCCPStateEnum.Disconnected : OCCPStateEnum.Connected;
            ss.NrOfSockets = Converters.ConvertRegistersShort(stationStatus, 5);
            return ss;
        }
        public static SocketMeasurement ReadSocketMeasurement(ModbusMaster master, byte socket)
        {
            var sm = new SocketMeasurement();
            var sm_part1 = master.ReadHoldingRegisters(socket, 300, 125);       // TODO 126
            var sm_part2 = master.ReadHoldingRegisters(socket, 1200, 16);
            //Console.WriteLine("---");
            //Console.WriteLine(HexDumper.ConvertToHexDump(sm_part1));
            //Console.WriteLine(HexDumper.ConvertToHexDump(sm_part2));

            sm.MeterState = Converters.ConvertRegistersShort(sm_part1, 0);
            sm.MeterTimestamp = Converters.ConvertRegistersLong(sm_part1, 1);

            switch (Converters.ConvertRegistersShort(sm_part1, 5))
            {
                case 0:
                    sm.MeterType = MeterType.RTU;
                    break;
                case 1:
                    sm.MeterType = MeterType.TCP_IP;
                    break;
                case 2:
                    sm.MeterType = MeterType.UDP;
                    break;
                case 3:
                    sm.MeterType = MeterType.P1;
                    break;
                case 4:
                    sm.MeterType = MeterType.Other;
                    break;
                default:
                    sm.MeterType = MeterType.UnknownType;
                    break;
            }

            sm.RealEnergyDeliveredL1 = Converters.ConvertRegistersDouble(sm_part1, 62);
            sm.RealEnergyDeliveredL2 = Converters.ConvertRegistersDouble(sm_part1, 66);
            sm.RealEnergyDeliveredL3 = Converters.ConvertRegistersDouble(sm_part1, 70);
            sm.RealEnergyDeliveredSum = Converters.ConvertRegistersDouble(sm_part1, 74);

            sm.Availability = Converters.ConvertRegistersShort(sm_part2, 0) == 1;
            sm.Mode3State = Converters.ConvertRegistersToString(sm_part2, 1, 5);

            sm.MaxCurrentValidTime = Converters.ConvertRegistersUInt32(sm_part2, 8);
            sm.MaxCurrent = Converters.ConvertRegistersFloat(sm_part2, 10);
            //Console.WriteLine(HexDumper.ConvertToHexDump(sm.MeterTimestamp));
            //Console.WriteLine(HexDumper.ConvertToHexDump((UInt64)1500));  
            //Console.WriteLine(HexDumper.ConvertToHexDump(sm.RealEnergyDeliveredSum));
            //Console.WriteLine(HexDumper.ConvertToHexDump((double)149290));                      
            return sm;
        }
    }
}
