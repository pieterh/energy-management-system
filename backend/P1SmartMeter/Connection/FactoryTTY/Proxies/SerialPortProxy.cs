using System.IO.Ports;
using P1SmartMeter.Connection.Proxies;

namespace P1SmartMeter.Connection.FactoryTTY.Proxies
{
    internal class SerialPortProxy : SerialPort, ISerialPort
    {
        public SerialPortProxy(string portName) : base(portName)
        {
        }
    }
}
