using System;
using System.Collections.Concurrent;
using EMS.Library;
using EMS.Library.TestableDateTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EMS;

public class Watchdog : BackgroundWorker, IWatchdog
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    internal readonly ConcurrentDictionary<IBackgroundWorker, Info> _workersToWatch = new ConcurrentDictionary<IBackgroundWorker, Info>();
    internal DateTimeOffset _lastCheck = DateTimeOffsetProvider.Now;

    public void Register(IBackgroundWorker bgWorkerToWatch, int interval)
    {
        _workersToWatch.AddOrUpdate(bgWorkerToWatch,
                        (bg) =>
                        {
                            var now = DateTimeOffsetProvider.Now;
                            return new Info(now, interval, (int)Math.Round((interval * 1.05) + 5, MidpointRounding.AwayFromZero));
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
            info.LastSeen = DateTimeOffsetProvider.Now;
        }
        else
        {
            var msg = "Unable to find bgWorker";
            Logger.Error(msg);
            throw new EMS.Library.Exceptions.ApplicationException(msg);
        }
    }

    /// <summary>
    /// Run every minute to check on the background workers
    /// </summary>
    protected override DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(120 * 1000);
    }

    protected override async Task DoBackgroundWork()
    {
        var now = DateTimeOffsetProvider.Now;
        var silentWorkers = _workersToWatch.Where((x) => (now - x.Value.LastSeen).TotalSeconds > x.Value.ExpectedIntervalSeconds).ToArray();

        if (silentWorkers.Any())
            Logger.Error("Watchdog found faulty tasks. Restarting them.");

        foreach (IBackgroundWorker worker in silentWorkers.Select((x) => x.Key))
        {
            _workersToWatch.Remove(worker, out _);
            Logger.Error("Watchdog restarting => {worker}", worker.GetType().Name);
            await worker.Restart().ConfigureAwait(false);
        }
        _lastCheck = now;
    }
}

internal record Info(DateTimeOffset FirstSeen, int RequestedIntervalSeconds, int ExpectedIntervalSeconds)
{
    internal DateTimeOffset LastSeen { get; set; } = FirstSeen;
}