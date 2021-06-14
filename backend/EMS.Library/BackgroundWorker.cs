using System;
using System.Threading;
using System.Threading.Tasks;

namespace EMS.Library
{
    public abstract class BackgroundWorker : BackgroundService, IBackgroundWorker
    {
        private Task _backgroundTask = null;
        public Task BackgroundTask { get { return _backgroundTask; } }

        protected abstract void DoBackgroundWork();
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            base.Dispose(disposing);

            if (disposing)
            {
                DisposeBackgroundTask();
            }

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
                    TokenSource.Cancel();
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

        protected override void Start()
        {
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
            }, TokenSource.Token);
        }

        protected virtual int Interval { get { return 2500; } }
        private void Run()
        {
            while (!StopRequested(Interval))
            {
                DoBackgroundWork();
            }

            if (TokenSource.Token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }
    }
}