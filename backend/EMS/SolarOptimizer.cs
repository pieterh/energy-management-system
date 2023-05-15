using System.Data;
using EMS.Library;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Adapter.Solar;
using EMS.Library.Cron;
using EMS.Library.Tasks;

namespace EMS;

public class SolarOptimizer : BackgroundService
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    static decimal _minTariffCutOff = (decimal)-0.126;
    private readonly IPriceProvider _priceProvider;
    private readonly ISolar _solar;

    private Task? _backgroundTask;
    private readonly Crontab _cron = new Crontab("55 * * * *");

    public SolarOptimizer(IPriceProvider priceProvider, ISolar solar)
    {
        _priceProvider = priceProvider;
        _solar = solar;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            DisposeBackgroundTask();
        }

        base.Dispose(disposing);
    }

    private void DisposeBackgroundTask()
    {
        if (_backgroundTask is not null)
        {
            // we need atleast a minimal wait for the background task to finish.
            // but we extend it when we are not beeing canceled
            TaskTools.Wait(_backgroundTask, 500);
            TaskTools.Wait(_backgroundTask, 4500, TokenSource.Token);
            _backgroundTask.Dispose();
            _backgroundTask = null;
        }
    }

    protected override Task Start()
    {
        _backgroundTask = Task.Run(async () =>
        {
            Thread.CurrentThread.Name = $"{this.GetType().Name} thread";
            Logger.Trace("BackgroundTask running");
            try
            {
                bool run;
                do
                {
                    await PerformCheck().ConfigureAwait(false);

                    var now = DateTimeOffset.Now;
                    var nextRun = _cron.GetNextOccurrence(now);
                    var sleeptimeMs = (int)((nextRun.UtcTicks - now.UtcTicks) / 10000);
                    Logger.Warn("Solar production => sleeping {sleepm}m, {sleeps}ms", (double)sleeptimeMs / 60000.0, sleeptimeMs);
                    run = !await StopRequested(sleeptimeMs).ConfigureAwait(false);
                } while (run);
            }
            catch (OperationCanceledException) { /* We expecting the cancelation exception and don't need to act on it */ }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unhandled exception in BackgroundTask");
                throw;
            }

            if (TokenSource.Token.IsCancellationRequested)
                Logger.Info("Canceled");

            Logger.Trace("BackgroundTask stopped -> stop requested {StopRequested}", StopRequested(0));
        }, TokenSource.Token)
            .ContinueWith((c, e) =>
                {
                    Logger.Error(c.Exception, "Task ended with an ignored exception" + Environment.NewLine);
                    Logger.Error("Solar task done 1 => {status}", c.Status);
                },
                null,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current
            )
            .ContinueWith((c, e) =>
                {
                    Logger.Warn("Solar task done 2 => {status}", c.Status);
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
        /* wait a bit for the background task in the case that it still is trying to connect */
        TaskTools.Wait(_backgroundTask, 10000);
        DisposeBackgroundTask();
    }

    internal async Task PerformCheck()
    {
        Logger.Warn("Solar production => start check");
        var tariff = _priceProvider.GetNextTariff();

        if (tariff is not null && tariff.TariffReturn <= _minTariffCutOff)
        {
            Logger.Warn("Solar production => {TariffReturn}", tariff.TariffReturn);
            var isForcedOff = await _solar.GetProductionStatus().ConfigureAwait(false);
            if (!isForcedOff)
            {
                Logger.Warn("Solar production => switch off");
                await _solar.StopProduction().ConfigureAwait(false);
            }
            else
                Logger.Warn("Solar production => already off");
        }
        else
        {
            var isForcedOff = await _solar.GetProductionStatus().ConfigureAwait(false);
            if (isForcedOff)
            {
                Logger.Warn("Solar production => switch on");
                await _solar.StartProduction().ConfigureAwait(false);
            }
            else
                Logger.Warn("Solar production => already on");
        }
    }
}

