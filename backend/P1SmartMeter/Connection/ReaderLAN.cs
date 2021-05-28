using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class ReaderLAN : Reader                                   //NOSONAR
    {
        private readonly string _host;
        private readonly int _port;
        private const int RECEIVE_BUFFER_SIZE = 2048;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private SocketAsyncEventArgs _receiveEventArgs;

        public ReaderLAN(string host, int port)
        {
            _host = host;
            _port = port;
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
            _receiveEventArgs?.Dispose();
            _receiveEventArgs = null;

            _stream?.Close();
            _stream?.Dispose();
            _stream = null;

            _tcpClient?.Close();
            _tcpClient?.Dispose();
            _tcpClient = null;
        }

        protected override void Start()
        {
            _tcpClient = new TcpClient(_host, _port)
            {
                ReceiveBufferSize = RECEIVE_BUFFER_SIZE,
                ReceiveTimeout = 30000
            };

            _stream = _tcpClient.GetStream();

            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(new Byte[RECEIVE_BUFFER_SIZE], 0, RECEIVE_BUFFER_SIZE);
            _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);

            if (!_stream.Socket.ReceiveAsync(_receiveEventArgs))
            {
                Console.WriteLine($"kuch1");
                ProcesReceivedData(_receiveEventArgs);
            }
        }

        protected override void Stop()
        {
            DisposeStream();
        }

        private void ProcesReceivedData(SocketAsyncEventArgs receiveEventArgs)
        {
            if (StopRequested(0)) { return; }

           
            var data = Encoding.ASCII.GetString(receiveEventArgs.MemoryBuffer.Span.Slice(0, receiveEventArgs.BytesTransferred));
            OnDataArrived(new DataArrivedEventArgs() { Data = data });

            if (!_stream.Socket.ReceiveAsync(receiveEventArgs))
            {
                ProcesReceivedData(receiveEventArgs);
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            ProcesReceivedData(e);
        }

        protected override void DoBackgroundWork()
        {
            if (!_stream.Socket.Connected)
            {
                Console.WriteLine($"Lost connection... stop");
                Stop();
                if (!StopRequested(10000))
                {
                    Console.WriteLine($"Lost connection... and try again");
                    Start();
                }
            }                
        }
    }
}
