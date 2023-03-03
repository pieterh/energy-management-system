﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class ReaderLAN : Reader                                   //NOSONAR
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
            if (!Disposed)
                TokenSource.Cancel();
            _backgroundTask?.Wait(5000);
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
            _backgroundTask = Task.Run(async () =>
            {
                Logger.Trace($"BackgroundTask running");
                try
                {
                    bool run;
                    do {
                        run = !await Connect().ConfigureAwait(false) && ! await StopRequested(2000).ConfigureAwait(false);
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

        private async Task<bool> Connect()
        {
            bool isConnected = false;
            try
            {
                Logger.Info($"Connecting {_host}:{_port}");
                /* should keep try connecting */
                _tcpClient = new TcpClient()               
                {
                    ReceiveBufferSize = RECEIVE_BUFFER_SIZE,
                    ReceiveTimeout = 30000
                };

                await _tcpClient.ConnectAsync(_host, _port, TokenSource.Token).ConfigureAwait(false);

                _stream = _tcpClient.GetStream();

                _receiveEventArgs = new SocketAsyncEventArgs();
                _receiveEventArgs.SetBuffer(new Byte[RECEIVE_BUFFER_SIZE], 0, RECEIVE_BUFFER_SIZE);
                _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);

                if (!_stream.Socket.ReceiveAsync(_receiveEventArgs))
                {
                    Logger.Trace($"kuch1");
                    await ProcesReceivedData(_receiveEventArgs).ConfigureAwait(false);
                }
                isConnected = true;
            }
            catch (SocketException se1) when (se1.SocketErrorCode == SocketError.TimedOut)
            {
                DisposeStream();
                Logger.Error("Socket timed out while connecting.");
            }
            catch(SocketException se2) 
            {
                DisposeStream();
                Logger.Error(se2, $"Socket exception {se2.Message}");
            }
            return isConnected;
        }

        private async Task ProcesReceivedData(SocketAsyncEventArgs receiveEventArgs)
        {
            if (await StopRequested(0).ConfigureAwait(false)) { return; }
            if (!_stream.Socket.Connected) { 
                await RestartConnection().ConfigureAwait(false); return;
            }
            
            if (receiveEventArgs.BytesTransferred <= 0)            
            {
                Logger.Error("No bytes received. Lost connection and retry.");
                await RestartConnection().ConfigureAwait(false);
                return;
            }

            var data = Encoding.ASCII.GetString(receiveEventArgs.MemoryBuffer.Span.Slice(0, receiveEventArgs.BytesTransferred));
            OnDataArrived(new DataArrivedEventArgs() { Data = data });

            if (!_stream.Socket.ReceiveAsync(receiveEventArgs))
            {
                if (receiveEventArgs.BytesTransferred <= 0)
                {
                    Logger.Error("No bytes received. Lost connection and retry..");
                    await RestartConnection().ConfigureAwait(false);
                    return;
                }

                await ProcesReceivedData(receiveEventArgs).ConfigureAwait(false);
            }
        }

        private async void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                await ProcesReceivedData(e).ConfigureAwait(false);
            }catch (Exception ex)
            {
                Logger.Error(ex, "While processing received data");
            }
        }

        private async Task RestartConnection()
        {
            Logger.Warn($"Restart connection... first stop");
            Stop();
            if (!await StopRequested(500).ConfigureAwait(false))
            {
                Logger.Warn($"and try again");
                Start();
            }
        }
    }
}
