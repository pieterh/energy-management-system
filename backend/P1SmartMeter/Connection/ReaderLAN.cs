using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    [SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
    public class ReaderLAN : Reader                                   
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _host;
        private readonly int _port;
        private const int RECEIVE_BUFFER_SIZE = 2048;
        private Task _backgroundTask;

        private Socket _socket;
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
            _backgroundTask?.Wait(5000);
            _backgroundTask?.Dispose();
            _backgroundTask = null;
        }

        private void DisposeStream()
        {
            Logger.Trace($"DisposeStream {_stream?.Socket.LocalEndPoint.ToString()}");
            _receiveEventArgs?.Dispose();
            _receiveEventArgs = null;

            _stream?.Close();
            _stream?.Dispose();
            _stream = null;

            if (_socket is not null)
            {
                if (_socket.Connected)
                    _socket.Disconnect(false);

                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }
        }

        protected override Task Start()
        {
            _backgroundTask = Task.Run(async () =>
            {
                Logger.Trace("BackgroundTask running");
                try
                {
                    bool run;
                    do
                    {
                        run = !Connect() && !await StopRequested(2000).ConfigureAwait(false);
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

            return _backgroundTask;
        }

        protected override void Stop()
        {
            TokenSource.Cancel();
            /* wait a bit for the background task in the case that it still is trying to connect */
            _backgroundTask.Wait(750);

            DisposeStream();
            DisposeBackgroundTask();
        }

        private bool Connect()
        {
            bool isConnected = false;
            try
            {
                Logger.Info($"Connecting {_host}:{_port}");

                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                _receiveEventArgs = new SocketAsyncEventArgs();
                _receiveEventArgs.SetBuffer(new Byte[RECEIVE_BUFFER_SIZE], 0, RECEIVE_BUFFER_SIZE);
                _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnCompleted);
                _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_host), _port);
                if (!_socket.ConnectAsync(_receiveEventArgs))
                {
                    Logger.Info($"Data {_receiveEventArgs.BytesTransferred},{_socket.Connected}");
                    OnCompleted(this, _receiveEventArgs);
                }

                isConnected = true;
                Logger.Trace($"Connected {isConnected},{_socket.Connected},{((IPEndPoint)_socket.LocalEndPoint)?.Port}");
            }
            catch (SocketException se1)
                when (se1.SocketErrorCode == SocketError.TimedOut ||
                      se1.SocketErrorCode == SocketError.ConnectionRefused)
            {
                DisposeStream();
                Logger.Error($"Socket error {se1.SocketErrorCode} while connecting.");
            }
            catch (SocketException se2)
            {
                DisposeStream();
                Logger.Error(se2, $"Unexpected socket exception while connecting.");
            }

            return isConnected;
        }

        private async void OnCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            try
            {
                var socket = eventArgs.ConnectSocket;
                Logger.Trace($"==> {eventArgs.LastOperation}");
                switch (eventArgs.LastOperation)
                {
                    case SocketAsyncOperation.Connect:
                        await ProcesConnect(eventArgs).ConfigureAwait(false);
                        break;
                    case SocketAsyncOperation.Receive:
                        await ProcesReceive(eventArgs).ConfigureAwait(false);
                        break;
                    default:
                        Logger.Error($"Received {eventArgs.LastOperation}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "While processing received data");
            }
        }

        private async Task ProcesConnect(SocketAsyncEventArgs connectEventArgs)
        {
            var socket = connectEventArgs.ConnectSocket;
            switch (connectEventArgs.SocketError)
            {
                case SocketError.Success:
                    Logger.Info($"{connectEventArgs.LastOperation}, port={((IPEndPoint)socket.LocalEndPoint).Port}, connected={socket.Connected}, socketError={connectEventArgs.SocketError}");
                    bool willRaiseEvent = socket.ReceiveAsync(connectEventArgs);
                    if (!willRaiseEvent)
                    {
                        await ProcesReceive(connectEventArgs).ConfigureAwait(false);
                    }
                    break;
                case SocketError.ConnectionRefused:
                    Logger.Error("Connection refused");
                    await RestartConnection().ConfigureAwait(false);
                    break;
                case SocketError.OperationAborted:
                    Logger.Info("Socket operation aborted");
                    break;
                default:
                    throw new SocketException((int)connectEventArgs.SocketError);
            }
        }

        private async Task ProcesReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            if (await StopRequested(0).ConfigureAwait(false)) { return; }
            var socket = receiveEventArgs.ConnectSocket;

            if (receiveEventArgs.BytesTransferred <= 0 || !socket.Connected)
            {
                Logger.Error($"No bytes received {receiveEventArgs.BytesTransferred}. Lost connection and retry. {((IPEndPoint)socket.LocalEndPoint).Port},{socket.Connected},{receiveEventArgs.SocketError}");
                await RestartConnection().ConfigureAwait(false);
                return;
            }

            var data = Encoding.ASCII.GetString(receiveEventArgs.MemoryBuffer.Span[..receiveEventArgs.BytesTransferred]);
            OnDataArrived(new DataArrivedEventArgs() { Data = data });

            // Start receiving more data
            if (!socket.ReceiveAsync(receiveEventArgs))
            {
                // completion was synch, so process immediatly
                await ProcesReceive(receiveEventArgs).ConfigureAwait(false);
            }
        }

        private async Task RestartConnection()
        {
            Logger.Info($"Restart connection...");
            Stop();

            Logger.Trace($"and try again");
            await StartAsync().ConfigureAwait(false);
        }
    }
}
