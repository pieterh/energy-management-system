using System.IO.Ports;

namespace P1SmartMeter.Connection.Proxies
{
    internal interface ISerialPort : IDisposable
    {
        int BaudRate { get; set; }
        Parity Parity { get; set; }
        StopBits StopBits { get; set; }
        int DataBits { get; set; }
        int ReadTimeout { get; set; }
        int ReadBufferSize { get; set; }

        event SerialDataReceivedEventHandler DataReceived;
        void Open();
        void Close();
        string ReadExisting();
    }
}
