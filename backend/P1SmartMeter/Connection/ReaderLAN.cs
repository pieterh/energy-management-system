using System;
using System.Net.Sockets;
using System.Text;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class ReaderLAN : Reader                                   //NOSONAR
    {
        private readonly string _host;
        private readonly int _port;
        private readonly byte[] _buffer = new byte[4096];
        private readonly Memory<byte> _memmoryBuffer ;

        private TcpClient _tcpClient;
        private NetworkStream _stream;

        public ReaderLAN(string host, int port)
        {
            _host = host;
            _port = port;

            _memmoryBuffer = _buffer.AsMemory(0, _buffer.Length);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeStream();
            }
        }

        private void DisposeStream()
        {
            _stream?.Close();
            _stream?.Dispose();
            _stream = null;

            _tcpClient?.Close();
            _tcpClient?.Dispose();
            _tcpClient = null;
        }

        protected override void Start()
        {
            _tcpClient = new TcpClient(_host, _port);
            _tcpClient.ReceiveBufferSize = 4096;
            _tcpClient.ReceiveTimeout = 30000;

            _stream = _tcpClient.GetStream();
        }

        protected override void Stop()
        {
            DisposeStream();
        }

        // would like to have an event when data arrives at the socket instead of this polling.... :-(
        protected override int Interval { get { return 200; } }
        protected override async void DoBackgroundWork()
        {
            try
            {
                if (_stream.DataAvailable)
                {
                    var nrCharsRead = await _stream.ReadAsync(_memmoryBuffer, TokenSource.Token);
                    var data = Encoding.ASCII.GetString(_memmoryBuffer.Span.Slice(0, nrCharsRead));
                    OnDataArrived(new DataArrivedEventArgs() { Data = data });                    
                }
            }
            catch (OperationCanceledException oce)
            {
                if (!StopRequested(0))
                    Logger.Error("Unexpected ", oce);
            }
        }
    }

}
