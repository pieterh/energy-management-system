using System.Diagnostics.CodeAnalysis;

namespace EMS.Library
{
    public abstract class BackgroundWorker : BackgroundService, IBackgroundWorker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Task? _backgroundTask;
        public Task? BackgroundTask { get { return _backgroundTask; } }

        protected abstract void DoBackgroundWork();
        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;
            base.Dispose(disposing);

            if (disposing)
            {
                DisposeBackgroundTask();
            }
        }

        [SuppressMessage("Code Analysis", "CA1031")]
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
            if (_backgroundTask == null)
                return;

            var timeOut = 5000;
            var waitingTime = 0;
            var delayTime = 150;

            while (Task.WhenAny(Task.Delay(delayTime), _backgroundTask).GetAwaiter().GetResult() != _backgroundTask && waitingTime < timeOut)
            {
                waitingTime += delayTime;
            }
        }

        protected override Task Start()
        {
            _backgroundTask = Task.Run(async () =>
            {
                Logger.Trace($"BackgroundTask running");
                try
                {
                    await Run().ConfigureAwait(false);
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

        protected virtual int Interval { get { return 2500; } }
        private async Task Run()
        {
            while (!await StopRequested(Interval).ConfigureAwait(false))
            {
                DoBackgroundWork();
            }

            if (TokenSource?.Token.IsCancellationRequested ?? false)
            {
                throw new OperationCanceledException();
            }
        }
    }
}