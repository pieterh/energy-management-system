using System.IO.Ports;
using P1SmartMeter.Connection.FactoryTTY.Proxies;
using P1SmartMeter.Connection.Proxies;

namespace P1SmartMeter.Connection.Factories
{
    internal interface ISerialPortFactory
    {
        string[] GetPortNames();
        ISerialPort CreateSerialPort(string portName);
    }

    internal class SerialPortFactory : ISerialPortFactory
    {
        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public ISerialPort CreateSerialPort(string portName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(portName);
            return new SerialPortProxy(portName);
        }
    }
}
