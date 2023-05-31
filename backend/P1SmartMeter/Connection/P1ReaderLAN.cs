using System.Net;
using System.Net.Sockets;
using EMS.Library.Tasks;
using P1SmartMeter.Connection.Proxies;

namespace P1SmartMeter.Connection;

enum ConnectionStatus
{
    Connecting,
    Connected,
    Disconnected
};

[SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
internal sealed class P1ReaderLAN : P1Reader
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private const int RECEIVE_BUFFER_SIZE = 2048;

    private readonly string _host;
    private readonly int _port;
    private readonly ISocketFactory _socketFactory;

    private Task? _backgroundTask;

    private ISocket? _socket;
    private ISocketAsyncEventArgs? _receiveEventArgs;

    public ConnectionStatus Status { get; internal set; } = ConnectionStatus.Disconnected;

    public P1ReaderLAN(string host, int port, ISocketFactory? socketFactory = null)
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
            DisposeBackgroundTask();
        }

        base.Dispose(disposing);
    }

    private void DisposeBackgroundTask()
    {
        if (_backgroundTask is not null)
        {
            // we need atleast a minimal wait for the background task to finish.
            // but we extend it when we are not beeing canceled
            TaskTools.Wait(_backgroundTask, 500);
            TaskTools.Wait(_backgroundTask, 4500, CancellationToken);
            _backgroundTask.Dispose();
            _backgroundTask = null;
        }
    }

    private void DisposeSocket()
    {
        _receiveEventArgs?.Dispose();
        _receiveEventArgs = null;

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

            if (CancellationToken.IsCancellationRequested)
                Logger.Info("Canceled");

            Logger.Trace("BackgroundTask stopped -> stop requested {StopRequested}", StopRequested(0));
        }, CancellationToken);

        return _backgroundTask;
    }

    protected override void Stop()
    {
        /* wait a bit for the background task in the case that it still is trying to connect */
        TaskTools.Wait(_backgroundTask, 10000);

        DisposeSocket();
        DisposeBackgroundTask();
        Status = ConnectionStatus.Disconnected;
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
                await RestartConnection().ConfigureAwait(false);
                break;
            case SocketError.OperationAborted:
                Logger.Info("Socket operation aborted");
                Status = ConnectionStatus.Disconnected;
                if (!TokenSource?.IsCancellationRequested ?? true)
                    await RestartConnection().ConfigureAwait(false);
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
            Logger.Error($"Lost connection and retry. {receiveEventArgs.SocketError}");
            await RestartConnection().ConfigureAwait(false);
            return;
        }

        var data = Encoding.ASCII.GetString(receiveEventArgs.MemoryBuffer.Span[..receiveEventArgs.BytesTransferred]);
        OnDataArrived(new DataArrivedEventArgs(data));

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
        TokenSource?.Cancel();      // NET 8 has CancelAsync...need to check that out
        Stop();

        Logger.Trace($"and try again");
        await StartAsync().ConfigureAwait(false);
    }
}