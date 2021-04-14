using System;
using System.Threading;
using System.Threading.Tasks;

namespace EMS.Library
{
    public abstract class BackgroundWorker : IBackgroundWorker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed = false;
        private Task _backgroundTask = null;

        private CancellationTokenSource _tokenSource = null;

        public Task BackgroundTask { get { return _backgroundTask; } }

        public CancellationTokenSource TokenSource { get => _tokenSource; private set => _tokenSource = value; }

        protected abstract void DoBackgroundWork();
        protected abstract void Start();
        protected abstract void Stop();

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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Trace($"Starting");
            this.Start();

            _tokenSource = new CancellationTokenSource();
            _backgroundTask = Task.Run(() =>
            {
                Logger.Trace($"BackgroundTask running");
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
                Logger.Trace($"BackgroundTask stopped -> stop requested {StopRequested(0)}");
            }, _tokenSource.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Stop();
            _tokenSource?.Cancel();
            return Task.CompletedTask;
        }


        private void Run()
        {
            while (!StopRequested(2500))
            {
                DoBackgroundWork();
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
            for (var i = 0; i < ms / 500; i++)
            {
                Thread.Sleep(500);
                if (_tokenSource?.Token == null || _tokenSource.Token.IsCancellationRequested)
                    return true;
            }
            return false;
        }


    }
}