using System;
using System.IO.Ports;
using System.Linq;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class ReaderTTY : Reader                                   //NOSONAR
    {
        private readonly string _usbPort;

        public ReaderTTY(string deviceName)
        {
            Logger.Trace($"Availble ports ->");
             
            SerialPort.GetPortNames().ToList().ForEach(x => {
                Logger.Trace($"Port '{x}'");
            });

            _usbPort = deviceName;
        }

        protected override void Run()
        {
            using (var str = new SerialPort(_usbPort))
            {
                str.BaudRate = 115200;
                str.Parity = Parity.None;
                str.StopBits = StopBits.One;
                str.DataBits = 8;
                str.ReadTimeout = 30000;
                str.ReadBufferSize = 4096;
                str.DataReceived += Str_DataReceived;
                str.Open();

                Logger.Info($"BackgroundTask run---");

                bool stopRequested;
                do
                {
                    stopRequested = !StopRequested(250);
                }
                while (!stopRequested);                

                str.Close();
            }

            if (_tokenSource.Token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        private void Str_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Logger.Info($"DataReceived!");
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();            
            Logger.Info($"BackgroundTask read {indata.Length} characters...");

            OnDataArrived(new DataArrivedEventArgs() { Data = indata });
        }
    }
}
