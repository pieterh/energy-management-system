using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class LANReader : IP1Interface                                   //NOSONAR
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed = false;
        private Task _backgroundTask = null;

        private CancellationTokenSource _tokenSource = null;

        public Task BackgroundTask { get { return _backgroundTask; } }

        private readonly string _host;
        private readonly int _port;

        public event EventHandler<IP1Interface.DataArrivedEventArgs> DataArrived;

        public LANReader(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Suppress finalization.
        }

        protected virtual void Dispose(bool disposing)
        {
            Logger.Trace($"Dispose({disposing}) _disposed {_disposed}");

            if (_disposed) return;

            if (disposing)
            {
                DisposeBackgroundTask();
                DisposeTokenSource();
            }

            _disposed = true;
            Logger.Trace($"Dispose({disposing}) done => _disposed {_disposed}");
        }

        private void DisposeBackgroundTask()
        {
            if (_backgroundTask == null) return;

            Logger.Trace($"backgroundTask status {_backgroundTask.Status}");
            if (!_backgroundTask.IsCompleted)
            {
                Logger.Warn($"Background worker beeing disposed while not completed.");

                try
                {
                    _tokenSource.Cancel();
                    WaitForBackgroundTaskToFinish();
                }
                catch (OperationCanceledException) { /* We expecting the cancelation exception and don't need to act on it */}
                catch (Exception ex)
                {
                    Logger.Error(ex, $"There was an error while performing an cancel on the background task {ex.ToString()}");
                }

                if (!_backgroundTask.IsCompleted)
                {
                    Logger.Warn($"Background worker did not react on cancel request.");
                }
            }

            try
            {
                if (_backgroundTask.IsCompleted)
                {
                    _backgroundTask.Dispose();
                }
                else
                    Logger.Error($"The background task has not completed. Unable to dispose.");
            }
            finally
            {
                _backgroundTask = null;
            }
        }

        private void WaitForBackgroundTaskToFinish()
        {
            var timeOut = 5000;
            var waitingTime = 0;
            var delayTime = 150;
            while (Task.WhenAny(Task.Delay(delayTime), _backgroundTask).GetAwaiter().GetResult() != _backgroundTask && waitingTime < timeOut)
            {
                waitingTime += delayTime;
            }
        }

        private void DisposeTokenSource()
        {
            if (_tokenSource == null) return;
            _tokenSource?.Cancel();
            _tokenSource = null;
        }

        public virtual void Start()
        {
            Logger.Info($"Starting");
            DisposeTokenSource();
            _tokenSource = new CancellationTokenSource();
            _backgroundTask = Task.Run(() =>
            {
                Logger.Info($"BackgroundTask running");
                try
                {
                    Run();
                }
                catch (OperationCanceledException) { /* We expecting the cancelation exception and don't need to act on it */ }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unhandled exception in BackgroundTask");
                    throw;
                }
                Logger.Info($"BackgroundTask stopped -> stop requested {StopRequested(0)}");
            }, _tokenSource.Token);
        }

        public virtual void Stop()
        {
            _tokenSource?.Cancel();
        }

        private void Run()
        {
            Logger.Info($"BackgroundTask run");

            using (var tcpClient = new TcpClient(_host, _port))
            {
                Logger.Info($"BackgroundTask connected");
                var bufje = new byte[4096];

                tcpClient.ReceiveBufferSize = 4096;
                tcpClient.ReceiveTimeout = 30000;
                using var s = tcpClient.GetStream();

                while (!StopRequested(250))
                {
                    Logger.Trace($"BackgroundTask reading!");
                    var nrCharsRead = s.Read(bufje, 0, bufje.Length);

                    Logger.Debug($"BackgroundTask read {nrCharsRead} bytes...");
                    var tmp = new byte[nrCharsRead];
                    Buffer.BlockCopy(bufje, 0, tmp, 0, nrCharsRead);

                    OnDataArrived(new DataArrivedEventArgs() { Data = Encoding.ASCII.GetString(tmp) });
                }

                s.Close();
            }

            if (_tokenSource.Token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        private bool StopRequested(int ms)
        {
            if (_tokenSource?.Token == null || _tokenSource.Token.IsCancellationRequested)
                return true;
            for (var i = 0; i < ms / 50; i++)
            {
                Thread.Sleep(50);
                if (_tokenSource?.Token == null || _tokenSource.Token.IsCancellationRequested)
                    return true;
            }
            return false;
        }

        protected void OnDataArrived(DataArrivedEventArgs e)
        {
            Logger.Debug($"data from stream!");
            EventHandler<DataArrivedEventArgs> handler = DataArrived;
            handler?.Invoke(this, e);
        }
    }

}
