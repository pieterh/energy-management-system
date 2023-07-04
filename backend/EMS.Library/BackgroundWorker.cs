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
    public Task? ExecuteTask { get { return _backgroundTask; } }

    private readonly IWatchdog _watchdog;
    public IWatchdog Watchdog => _watchdog;


    private bool _isRestarting;
    protected BackgroundWorker()
    {
#pragma warning disable S3060
        if (this is IWatchdog t)
            _watchdog = t;
        else
            throw new EMS.Library.Exceptions.ApplicationException("Only watchdog can be a worker without a wathdog ;-)");
#pragma warning restore
    }

    protected BackgroundWorker(IWatchdog watchdog)
    {
        _watchdog = watchdog;
    }

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
        if (!_isRestarting)
            Watchdog.Register(this, GetInterval());

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
                Logger.Error(ex, "Unhandled exception in BackgroundTask" + Environment.NewLine);
                throw;
            }

            Logger.Trace("BackgroundWorker {name} stopped -> stop requested {StopRequested}", this.GetType().Name, StopRequested(0));
            if (CancellationToken.IsCancellationRequested)
            {
                Logger.Info("Canceled - {name}", this.GetType().Name);
            }
            else
                Watchdog.Tick(this);
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
        );
        return _backgroundTask;
    }

    protected override void Stop()
    {
        TokenSource?.Cancel();
        if (!_isRestarting)
            Watchdog.Unregister(this);
        TaskTools.Wait(_backgroundTask, 2000);
        DisposeBackgroundTask();
    }

    public virtual async Task Restart(bool useSubtask = true)
    {
        if (useSubtask)
        {
            _ = Task.Factory.StartNew(action: async () =>
            {
                Logger.Warn("Restarting - {name} - in new task", this.GetType().Name);
                await Restarter().ConfigureAwait(false);
            }, CancellationToken, TaskCreationOptions.None, TaskScheduler.Current);
        }
        else
            await Restarter().ConfigureAwait(false);
    }

    private async Task Restarter()
    {
        if (!CancellationToken.IsCancellationRequested)
        {
            Logger.Warn("Restarting - {name}", this.GetType().Name);
            try
            {
                _isRestarting = true;
                await StopAsync(CancellationToken.None).ConfigureAwait(false);
                await StartAsync().ConfigureAwait(false);
            }
            finally
            {
                _isRestarting = false;
            }
        }
        else
            Logger.Warn("Restarting - {name} - cancellation is already requested", this.GetType().Name);
    }


    private const int _intervalms = 2500;
    /// <summary>
    /// The default next occurence is within 2500ms from now
    /// </summary>
    protected virtual DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(_intervalms);
    }

    /// <summary>
    /// The failsafe time
    /// </summary>
    /// <returns></returns>
    protected virtual int GetInterval()
    {
        return _intervalms;
    }

    protected virtual void WatchDogTick()
    {
        if (!CancellationToken.IsCancellationRequested)
            Watchdog.Tick(this);
    }

    private async Task Run()
    {
        bool run;
        do
        {
            await DoBackgroundWork().ConfigureAwait(false);

            var nextRun = GetNextOccurrence();
            var sleeptimeMs = (int)((nextRun.UtcTicks - DateTimeOffsetProvider.Now.UtcTicks) / 10000);
            Logger.Trace("BackgroundWorker {name} => sleeping {sleepm}m, {sleeps}ms", this.GetType().Name, (double)sleeptimeMs / 60000.0, sleeptimeMs);
            run = !await StopRequested(sleeptimeMs).ConfigureAwait(false);
        } while (run);

        //??
        if (!CancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }
    }
}