using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using EMS.Library.Tasks;
using EMS.Library.TestableDateTime;

namespace EMS.Library;

public abstract class BackgroundWorker : BackgroundService, IBackgroundWorker
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private Task? _backgroundTask;
    public Task? BackgroundTask { get { return _backgroundTask; } }

    protected abstract Task DoBackgroundWork();
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
        if (_backgroundTask is null) return;

        Logger.Trace($"backgroundTask status {_backgroundTask.Status}");
        if (!_backgroundTask.IsCompleted)
        {
            Logger.Warn("Background worker beeing disposed while not completed. {name}", this.GetType().Name);

            try
            {
                TokenSource?.Cancel();
                TaskTools.Wait(_backgroundTask, 5000);
            }
            catch (OperationCanceledException) { /* We expecting the cancelation exception and don't need to act on it */}
            catch (Exception ex)
            {
                Logger.Error(ex, "There was an error while performing an cancel on the background task {message}", ex.Message);
            }

            if (!_backgroundTask.IsCompleted)
            {
                Logger.Warn("Background worker did not react on cancel request.{name}", this.GetType().Name);
            }
        }

        try
        {
            if (_backgroundTask.IsCompleted)
            {
                _backgroundTask.Dispose();
            }
            else
                Logger.Error("The background task has not completed. Unable to dispose.{name}", this.GetType().Name);
        }
        finally
        {
            _backgroundTask = null;
        }
    }

    protected override Task Start()
    {
        _backgroundTask = Task.Run(async () =>
        {
            Logger.Trace("BackgroundWorker {name} running", this.GetType().Name);
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

            if (CancellationToken.IsCancellationRequested)
                Logger.Info("Canceled - {name}", this.GetType().Name);

            Logger.Trace("BackgroundWorker {name} stopped -> stop requested {StopRequested}", this.GetType().Name, StopRequested(0));
        }, CancellationToken)
        .ContinueWith((c, e) =>
            {
                Logger.Error(c.Exception, "Task ended with an ignored exception {name}" + Environment.NewLine, this.GetType().Name);
                Logger.Error("BackgroundWorker {name} done 1 => {status}", this.GetType().Name, c.Status);
            },
            null,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Current
        )
        .ContinueWith((c, e) =>
            {
                Logger.Warn("BackgroundWorker {name} - done 2 => {status}", this.GetType().Name, c.Status);
            },
            null,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Current
        );
        return _backgroundTask;
    }

    protected override void Stop()
    {
        TokenSource?.Cancel();

        TaskTools.Wait(_backgroundTask, 2000);
    }

    public virtual async Task Restart()
    {
        Logger.Warn("Restarting - {name}", this.GetType().Name);
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        await StartAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// The default interval is 2500ms
    /// </summary>
    protected virtual DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(2500);
    }

    private async Task Run()
    {
        bool run;
        do
        {
            await DoBackgroundWork().ConfigureAwait(false);

            var nextRun = GetNextOccurrence();
            var sleeptimeMs = (int)((nextRun.UtcTicks - DateTimeOffsetProvider.Now.UtcTicks) / 10000);
            Logger.Warn("BackgroundWorker {name} => sleeping {sleepm}m, {sleeps}ms", this.GetType().Name, (double)sleeptimeMs / 60000.0, sleeptimeMs);
            run = !await StopRequested(sleeptimeMs).ConfigureAwait(false);
        } while (run);

        //??
        if (!CancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
    }
}