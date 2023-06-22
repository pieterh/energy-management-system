using System.Net;
using System.Net.Sockets;

using EMS.Library;
using EMS.Library.Tasks;
using EMS.Library.TestableDateTime;
using P1SmartMeter.Connection.Proxies;

namespace P1SmartMeter.Connection;

enum ConnectionStatus
{
    Connecting,
    Connected,
    Disconnected,
    Reconnecting
};

[SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
internal sealed class P1ReaderLAN : P1Reader
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private const int RECEIVE_BUFFER_SIZE = 2048;

    private readonly string _host;
    private readonly int _port;
    private readonly ISocketFactory _socketFactory;

    private ISocket? _socket;
    private ISocketAsyncEventArgs? _receiveEventArgs;

    public ConnectionStatus Status { get; internal set; } = ConnectionStatus.Disconnected;

    public P1ReaderLAN(string host, int port, IWatchdog watchdog, ISocketFactory? socketFactory = null) : base(watchdog)
    {
        _host = host;
        _port = port;
        _socketFactory = socketFactory ?? new SocketFactory();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            DisposeSocket();
        }

        base.Dispose(disposing);
    }

    private void DisposeSocket()
    {
        _receiveEventArgs?.Dispose();
        _receiveEventArgs = null;

        if (_socket is not null)
        {
            try
            {
                if (_socket.Connected)
                    _socket.Disconnect(false);
            }
            catch (SocketException) { /* ignore socket exception when disconnecting / disposing */}
            _socket.Close();
            _socket.Dispose();
            _socket = null;
        }
    }

    protected override async Task Start()
    {
        Connect();
        await base.Start().ConfigureAwait(false);
    }

    protected override void Stop()
    {
        Status = ConnectionStatus.Disconnected;
        DisposeSocket();
        base.Stop();
    }

    private const int _intervalms = 10000;
    private const int _watchdogms = 15000;
    protected override DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(_intervalms);
    }

    protected override int GetInterval()
    {
        return _watchdogms;
    }

    protected override async Task DoBackgroundWork()
    {
        if (Status == ConnectionStatus.Reconnecting)
        {
            Logger.Warn("Status singnaled reconnecting...");
            Status = ConnectionStatus.Disconnected;

            await Restart(useSubtask: true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns true if the connection setup is started
    /// </summary>
    /// <returns></returns>
    internal bool Connect()
    {
        bool isConnecting = false;
        try
        {
            Logger.Info($"Connecting {_host}:{_port}");

            _socket = _socketFactory.CreateSocket(SocketType.Stream, ProtocolType.Tcp);
            _receiveEventArgs = _socketFactory.CreateSocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(new Byte[RECEIVE_BUFFER_SIZE], 0, RECEIVE_BUFFER_SIZE);
            _receiveEventArgs.Completed += new EventHandler<ISocketAsyncEventArgs>(OnCompleted);
            _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_host), _port);

            Status = ConnectionStatus.Connecting;

            CancellationToken.Register(() =>
            {
                if (Status == ConnectionStatus.Connecting && _socket is not null)
                {
                    Logger.Info("Cancellation requested while connecting. Cancel the ConnectAsync.");
                    _socket.CancelConnectAsync(_receiveEventArgs);
                }
            });

            if (!_socket.ConnectAsync(_receiveEventArgs))
            {
                Logger.Info($"Data {_receiveEventArgs.BytesTransferred},{_socket.Connected}");
                OnCompleted(this, _receiveEventArgs);
            }

            var port = _socket != null && _socket.LocalEndPoint != null ? ((IPEndPoint)_socket.LocalEndPoint).Port : -1;
            isConnecting = true;
            Logger.Trace($"Connecting from local port {port}");
        }
        catch (SocketException se1)
            when (se1.SocketErrorCode == SocketError.TimedOut ||
                  se1.SocketErrorCode == SocketError.ConnectionRefused)
        {
            DisposeSocket();
            Logger.Error($"Socket error {se1.SocketErrorCode} while connecting.");
        }
        catch (SocketException se2)
        {
            DisposeSocket();
            Logger.Error(se2, $"Unexpected socket exception while connecting.");
        }

        return isConnecting;
    }

    [SuppressMessage("Code Analysis", "CA1031")]
    private async void OnCompleted(object? sender, ISocketAsyncEventArgs eventArgs)
    {
        try
        {
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
        catch (SocketException se)
        {
            Logger.Error($"SocketException {se.Message}, while processing received data");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "While processing received data");
        }
    }

    private async Task ProcesConnect(ISocketAsyncEventArgs connectEventArgs)
    {
        var socket = connectEventArgs.ConnectSocket;
        switch (connectEventArgs.SocketError)
        {
            case SocketError.Success:
                var port = socket != null && socket.LocalEndPoint != null ? ((IPEndPoint)socket.LocalEndPoint).Port : -1;
                var connected = socket?.Connected;
                Logger.Info($"{connectEventArgs.LastOperation}, port={port}, connected={connected}, socketError={connectEventArgs.SocketError}");
                Status = ConnectionStatus.Connected;

                var willRaiseEvent = socket?.ReceiveAsync(connectEventArgs);
                if (willRaiseEvent.HasValue && !willRaiseEvent.Value)
                {
                    await ProcesReceive(connectEventArgs).ConfigureAwait(false);
                }
                break;
            case SocketError.ConnectionRefused:
            case SocketError.TimedOut:
                Logger.Error("Connection {SocketError}", connectEventArgs.SocketError);
                Status = ConnectionStatus.Disconnected;
                RestartConnection();
                break;
            case SocketError.OperationAborted:
                Logger.Info("Socket operation aborted");
                Status = ConnectionStatus.Disconnected;
                if (!TokenSource?.IsCancellationRequested ?? true)
                    RestartConnection();
                else
                    Logger.Info("Cancellation requested. Not restarting.");
                break;
            default:
                throw new SocketException((int)connectEventArgs.SocketError);
        }
    }

    private async Task ProcesReceive(ISocketAsyncEventArgs receiveEventArgs)
    {
        if (await StopRequested(0).ConfigureAwait(false)) { return; }
        var socket = receiveEventArgs.ConnectSocket;

        if (receiveEventArgs.BytesTransferred <= 0 || socket == null || !socket.Connected)
        {
            if (!TokenSource?.IsCancellationRequested ?? false)
            {
                Logger.Error($"Lost connection and retry. {receiveEventArgs.SocketError}");
                RestartConnection();
            }
            return;
        }

        if (receiveEventArgs.BytesTransferred > 0)
        {
            var data = Encoding.ASCII.GetString(receiveEventArgs.MemoryBuffer.Span[..receiveEventArgs.BytesTransferred]);
            OnDataArrived(new DataArrivedEventArgs(data));
        }

        // Start receiving more data
        if (!socket.ReceiveAsync(receiveEventArgs))
        {
            // completion was synch, so process immediatly
            await ProcesReceive(receiveEventArgs).ConfigureAwait(false);
        }
    }

    private void RestartConnection()
    {
        Logger.Info("Set status to reconnect...");
        Status = ConnectionStatus.Reconnecting;
    }
}