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
            while (!StopRequested(2500))
            {
                Console.WriteLine("Doing something usefull");
                ShowSocketMeasurement();
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
                Console.WriteLine("Name           {0}", pi.Name);
                Console.WriteLine("Manufacterer   {0}", pi.Manufacterer);
                Console.WriteLine("Table version  {0}", pi.TableVersion);
                Console.WriteLine("Station serial {0}", pi.StationSerial);
                Console.WriteLine("Date           {0} {1}", pi.DateTimeLocal.ToString(), pi.DateTimeLocal.Kind);
                Console.WriteLine("Date           {0} {1}", pi.DateTimeUtc.ToString(), pi.DateTimeUtc.Kind);
                Console.WriteLine("Uptime         {0}", pi.Uptime);
                Console.WriteLine("Up since       {0} {1}", pi.UpSinceUtc.ToString(), pi.UpSinceUtc.Kind);
                Console.WriteLine("Timezone       {0}", pi.StationTimezone);
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
            var stationStatus = master.ReadHoldingRegisters(200, 1100, 6);
            //var temparature = Converters.ConvertRegistersFloat(stationStatus, 2);                
            var occpState = Converters.ConvertRegistersShort(stationStatus, 4);
            Console.WriteLine($"OCCP {occpState}");
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
            Console.Write($"MeterState {sm.MeterState}");
            Console.Write($"MeterTimestamp {sm.MeterTimestamp}");
            Console.Write($"MeterType {sm.MeterType}");
            Console.Write($"Availability {sm.Availability}");
            Console.Write($"Mode3State {sm.Mode3State}");
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
            result.DateTimeLocal = new DateTime(result.DateTimeUtc.AddMinutes(result.StationTimezone).Ticks, DateTimeKind.Local);
            result.Uptime = Converters.ConvertRegistersLong(pi, 74);
            //result.UpSinceUtc = DateTime.UtcNow.AddMilliseconds(0 - result.Uptime);
            result.StationTimezone = Converters.ConvertRegistersShort(pi, 78);
            return result;
        }
        public static SocketMeasurement ReadSocketMeasurement(ModbusMaster master, byte socket)
        {
            var sm = new SocketMeasurement();
            var sm_part1 = master.ReadHoldingRegisters(socket, 300, 125);
            var sm_part2 = master.ReadHoldingRegisters(socket, 1200, 125);
            sm.MeterState = Converters.ConvertRegistersShort(sm_part2, 0);
            sm.MeterTimestamp = Converters.ConvertRegistersLong(sm_part2, 1);
            sm.MeterType = Converters.ConvertRegistersShort(sm_part2, 5);
            sm.Availability = Converters.ConvertRegistersShort(sm_part2, 0) == 1;
            sm.Mode3State = Converters.ConvertRegistersToString(sm_part2, 1, 5);
            return sm;
        }
    }
}
