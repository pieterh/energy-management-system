using System.IO.Ports;
using P1SmartMeter.Connection.Factories;
using P1SmartMeter.Connection.Proxies;

namespace P1SmartMeter.Connection
{
    [SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
    internal sealed class P1ReaderTTY : P1Reader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _usbPort;
        private readonly ISerialPortFactory _serialPortFactory;

        private ISerialPort? _serialPort;

        public P1ReaderTTY(string deviceName, ISerialPortFactory? serialPortFactory = null)
        {
            Logger.Trace($"Availble ports ->");

            _usbPort = deviceName;
            _serialPortFactory = serialPortFactory ?? new SerialPortFactory();

            _serialPortFactory.GetPortNames().ToList().ForEach(x =>
            {
                Logger.Trace($"Port '{x}'");
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                DisposeSerialPort();
            }
        }

        private void DisposeSerialPort()
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
            _serialPort = null;
        }

        protected override Task Start()
        {
            Logger.Info($"BackgroundTask start");

            _serialPort = _serialPortFactory.CreateSerialPort(_usbPort);
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataBits = 8;
            _serialPort.ReadTimeout = 30000;
            _serialPort.ReadBufferSize = 409;

            _serialPort.DataReceived += Str_DataReceived;
            _serialPort.Open();

            Logger.Info($"BackgroundTask has started");
            return Task.CompletedTask;
        }

        protected override void Stop()
        {
            DisposeSerialPort();
        }

        private void Str_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Logger.Info($"DataReceived!");
            ISerialPort sp = (ISerialPort)sender;
            string indata = sp.ReadExisting();
            Logger.Info($"BackgroundTask read {indata.Length} characters...");

            OnDataArrived(new DataArrivedEventArgs(indata));
        }
    }
}
