using System.Data;
using EMS.Library;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Adapter.Solar;
using EMS.Library.Cron;
using EMS.Library.Exceptions;
using EMS.Library.Tasks;

namespace EMS;

public class SolarOptimizer : BackgroundWorker
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    static decimal _minTariffCutOff = (decimal)-0.126;
    private readonly IPriceProvider _priceProvider;
    private readonly ISolar _solar;

    private readonly Crontab _cron = new Crontab("55 * * * *");
    private readonly int _watchdogms = 62 * 60 * 1000;

    public SolarOptimizer(IPriceProvider priceProvider, ISolar solar, IWatchdog watchdog) : base(watchdog)
    {
        _priceProvider = priceProvider;
        _solar = solar;
    }

    protected override DateTimeOffset GetNextOccurrence()
    {
        var now = DateTimeOffset.Now;
        var nextRun = _cron.GetNextOccurrence(now);
        return nextRun;
    }

    protected override int GetInterval()
    {
        return _watchdogms;
    }

    internal async Task PerformCheck()
    {
        Logger.Debug("Solar production => start check");
        var tariff = _priceProvider.GetNextTariff();

        if (tariff is not null && tariff.TariffReturn <= _minTariffCutOff)
        {
            Logger.Warn("Solar production => {TariffReturn}", tariff.TariffReturn);
            var isForcedOff = await _solar.GetProductionStatus(CancellationToken).ConfigureAwait(false);
            if (!isForcedOff)
            {
                Logger.Info("Solar production => switch off");
                await _solar.StopProduction(CancellationToken).ConfigureAwait(false);
            }
            else
                Logger.Debug("Solar production => already off");
        }
        else
        {
            var isForcedOff = await _solar.GetProductionStatus(CancellationToken).ConfigureAwait(false);
            if (isForcedOff)
            {
                Logger.Info("Solar production => switch on");
                await _solar.StartProduction(CancellationToken).ConfigureAwait(false);
            }
            else
                Logger.Debug("Solar production => already on");
        }
    }

    protected override async Task DoBackgroundWork()
    {
        try
        {
            await PerformCheck().ConfigureAwait(false);
            WatchDogTick();
        }
        catch (CommunicationException ce)
        {
            Logger.Error("There was a problem communicating {message}", ce.Message);
        }
        catch (TaskCanceledException)
        {
            if (CancellationToken.IsCancellationRequested)
                Logger.Info("Task cancelled");
            else
                Logger.Warn("Unexpected task cancellation");
        }
    }
}