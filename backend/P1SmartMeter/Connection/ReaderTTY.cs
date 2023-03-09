﻿using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class ReaderTTY : Reader                                   //NOSONAR
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _usbPort;
        private SerialPort _serialPort;

        public ReaderTTY(string deviceName)
        {
            Logger.Trace($"Availble ports ->");

            SerialPort.GetPortNames().ToList().ForEach(x =>
            {
                Logger.Trace($"Port '{x}'");
            });

            _usbPort = deviceName;
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

            _serialPort = new SerialPort(_usbPort)
            {
                BaudRate = 115200,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                ReadTimeout = 30000,
                ReadBufferSize = 4096
            };
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
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Logger.Info($"BackgroundTask read {indata.Length} characters...");

            OnDataArrived(new DataArrivedEventArgs() { Data = indata });
        }

    }
}
