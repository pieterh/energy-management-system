using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EMS.Library;
using EMS.Library.TestableDateTime;
using EMS.Library.Cron;

namespace EMS;

public class Watchdog : BackgroundWorker, IWatchdog
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    internal readonly ConcurrentDictionary<IBackgroundWorker, Info> _workersToWatch = new ConcurrentDictionary<IBackgroundWorker, Info>();
    internal DateTimeOffset _lastCheck = DateTimeOffsetProvider.Now;

    private readonly Crontab _cron = new Crontab("0,30 * * * * *", true);
    private readonly int _intervalms = -1;

    public Watchdog() : base()
    {

    }

    public void Register(IBackgroundWorker bgWorkerToWatch, int interval)
    {
        _workersToWatch.AddOrUpdate(bgWorkerToWatch,
                        (bg) =>
                        {
                            var now = DateTimeOffsetProvider.Now;

                            // when a negative interval is given, we will not watch the health of the worker
                            if (interval > 0)
                                return new Info(now, interval, (int)Math.Round((interval * 1.05), MidpointRounding.AwayFromZero)); // allow for 5% slack
                            else
                                return new Info(now, interval, interval);   
                        },
                        (bg, existingInfo) =>
                        {
                            existingInfo.LastSeen = DateTimeOffsetProvider.Now;
                            return existingInfo;
                        });
    }

    public void Unregister(IBackgroundWorker bgWorkerUnwatch)
    {
        _workersToWatch.Remove(bgWorkerUnwatch, out _);
    }

    public void Tick(IBackgroundWorker bgWorkerTicked)
    {
        if (_workersToWatch.TryGetValue(bgWorkerTicked, out var info))
        {
            info.LastSeen = DateTimeOffsetProvider.Now.UtcDateTime;
        }
        else
        {
            var msg = "Unable to find bgWorker";
            Logger.Error(msg);
            throw new EMS.Library.Exceptions.ApplicationException(msg);
        }
    }

    protected override DateTimeOffset GetNextOccurrence()
    {
        var now = DateTimeOffset.Now;
        var nextRun = _cron.GetNextOccurrence(now);
        return nextRun;
    }

    protected override int GetInterval()
    {
        return _intervalms;
    }

    protected override async Task DoBackgroundWork()
    {
        await PerformCheck().ConfigureAwait(false);
    }

    internal async Task PerformCheck()
    {
        var now = DateTimeOffsetProvider.Now.UtcDateTime;
        var silentWorkers = _workersToWatch.Where((x) => x.Value.ExpectedIntervalMilliseconds > 0 && (now - x.Value.LastSeen).TotalMilliseconds > x.Value.ExpectedIntervalMilliseconds).ToArray();

        if (silentWorkers.Any())
            Logger.Error("Watchdog found faulty tasks. Restarting them.");
        else
            Logger.Debug("Watchdog has {nr} tasks checked.", _workersToWatch.Count);

        foreach (var silentWorker in silentWorkers)
        {
#pragma warning disable CA1031 // we need to make sure no exception gets through
            var worker = silentWorker.Key;
            try
            {
                if (worker != this)
                {
                    Logger.Error("Watchdog restarting => {worker}, {seen}, {expected}, {actual}",
                        worker.GetType().Name, silentWorker.Value.LastSeen.ToString("O"),
                        silentWorker.Value.ExpectedIntervalMilliseconds, (now - silentWorker.Value.LastSeen).TotalMilliseconds);

                    await worker.Restart(false).ConfigureAwait(false);
                }
                else
                {
                    Logger.Error("Watchdog is a bit late.... => {worker}, {seen}, {expected}, {actual}",
                        worker.GetType().Name, silentWorker.Value.LastSeen.ToString("O"),
                        silentWorker.Value.ExpectedIntervalMilliseconds, (now - silentWorker.Value.LastSeen).TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "There was an error restarting failed worker {worker}" + Environment.NewLine, worker.GetType().Name);
            }
#pragma warning restore
        }
        _lastCheck = now;
    }
}

internal record Info(DateTimeOffset FirstSeen, int RequestedIntervalMilliseconds, int ExpectedIntervalMilliseconds)
{
    internal DateTimeOffset LastSeen { get; set; } = FirstSeen;
}