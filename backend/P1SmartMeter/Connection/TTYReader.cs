using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class TTYReader : IP1Interface                                   //NOSONAR
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed = false;
        private Task _backgroundTask = null;

        private CancellationTokenSource _tokenSource = null;

        public Task BackgroundTask { get { return _backgroundTask; } }

        private readonly string _usbPort;
        public event EventHandler<IP1Interface.DataArrivedEventArgs> DataArrived;

        public TTYReader(string deviceName)
        {
            Logger.Trace($"Availble ports ->");
             
            SerialPort.GetPortNames().ToList().ForEach(x => {
                Logger.Trace($"Port '{x}'");
            });

            _usbPort = deviceName;
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
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        public virtual void Start()
        {
            Logger.Info($"Starting+++");

            _tokenSource = new CancellationTokenSource();
            _backgroundTask = Task.Run(() =>
            {
                Logger.Info($"BackgroundTask running +++");
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
                Logger.Info($"BackgroundTask++ stopped -> stop requested {StopRequested(0)}");
            }, _tokenSource.Token);
        }

        public virtual void Stop()
        {
            if (_tokenSource == null) return;
            _tokenSource?.Cancel();
            // wait for background task to finish. but not to long...
            Task.WaitAll(new Task[] { BackgroundTask }, 500);
        }

        private void Run()
        {
            using (var str = new SerialPort(_usbPort))
            {
                str.BaudRate = 115200;
                str.Parity = Parity.None;
                str.StopBits = StopBits.One;
                str.DataBits = 8;
                str.ReadTimeout = 30000;
                str.ReadBufferSize = 4096;
                str.DataReceived += Str_DataReceived;
                str.Open();

                Logger.Info($"BackgroundTask run---");

                bool stopRequested;
                do
                {
                    stopRequested = !StopRequested(250);
                }
                while (!stopRequested);                

                str.Close();
            }

            if (_tokenSource.Token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        private void Str_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Logger.Info($"DataReceived!");
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();            
            Logger.Info($"BackgroundTask read {indata.Length} characters...");

            OnDataArrived(new DataArrivedEventArgs() { Data = indata });
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
            Logger.Info($"data from stream!");
            EventHandler<DataArrivedEventArgs> handler = DataArrived;
            handler?.Invoke(this, e);
        }
    }
}
