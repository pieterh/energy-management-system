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
        private Task _backgroundTask;

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
            base.Dispose(disposing);
            if (disposing)
            {
                DisposeStream();
                DisposeBackgroundTask();
            }
        }

        private void DisposeBackgroundTask()
        {
            _backgroundTask?.Dispose();
            _backgroundTask = null;
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
            _backgroundTask = Task.Run(() =>
            {
                Logger.Trace($"BackgroundTask running");
                try
                {
                    bool run;
                    do {
                        run = !Connect() && !StopRequested(5000);
                    }
                    while (run);                   
                }
                catch (OperationCanceledException) { /* We expecting the cancelation exception and don't need to act on it */ }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unhandled exception in BackgroundTask");
                    throw;
                }
                Logger.Trace($"BackgroundTask stopped -> stop requested {StopRequested(0)}");
            }, TokenSource.Token);            
        }

        protected override void Stop()
        {
            DisposeStream();
            DisposeBackgroundTask();
        }

        private bool Connect()
        {
            bool isConnected = false;
            try
            {
                /* should keep try connecting */
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
                    Logger.Trace($"kuch1");
                    ProcesReceivedData(_receiveEventArgs);
                }
                isConnected = true;
            }
            catch (SocketException)
            {
                Logger.Trace($"nope");
            }
            return isConnected;
        }

        private void ProcesReceivedData(SocketAsyncEventArgs receiveEventArgs)
        {
            if (StopRequested(0)) { return; }
            if (!_stream.Socket.Connected) { RestartConnection(); return; }
            
            if (receiveEventArgs.BytesTransferred <= 0)            
            {
                Logger.Error("No bytes received. Lost connection and retry.");
                RestartConnection();
                return;
            }

            var data = Encoding.ASCII.GetString(receiveEventArgs.MemoryBuffer.Span.Slice(0, receiveEventArgs.BytesTransferred));
            OnDataArrived(new DataArrivedEventArgs() { Data = data });

            if (!_stream.Socket.ReceiveAsync(receiveEventArgs))
            {
                if (receiveEventArgs.BytesTransferred <= 0)
                {
                    Logger.Error("No bytes received. Lost connection and retry..");
                    RestartConnection();
                    return;
                }

                ProcesReceivedData(receiveEventArgs);
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            ProcesReceivedData(e);
        }

        private void RestartConnection()
        {
            Logger.Warn($"Restart connection... first stop");
            Stop();
            if (!StopRequested(500))
            {
                Logger.Warn($"and try again");
                Start();
            }
        }
    }
}
