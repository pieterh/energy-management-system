using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EMS.DataStore;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.SmartMeterAdapter;
using EMS.Library.Adapter.Solar;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Core;
using EMS.Library.TestableDateTime;
using EMS.Library.Exceptions;

namespace EMS;

[SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
public class HEMSCore : BackgroundWorker, IHEMSCore
{
    private static readonly NLog.Logger LoggerChargingState = NLog.LogManager.GetLogger("chargingstate");
    private static readonly NLog.Logger LoggerChargingcost = NLog.LogManager.GetLogger("chargingcost");
    private readonly Compute _compute;

    private readonly ILogger Logger;

    private readonly ISmartMeterAdapter _smartMeter;
    private readonly IChargePoint _chargePoint;
    private readonly ISolar _solar;
    private readonly IPriceProvider _priceProvider;

    private const int _intervalms = 10000; //ms

    public ChargeControlInfo ChargeControlInfo { get { return _compute.Info; } }

    private bool _disposed;
    private SolarOptimizer? _solarOptimizerService;

    public ChargingMode ChargingMode
    {
        get => _compute.Mode;
        set => _compute.Mode = value;
    }

    public HEMSCore(ILogger<HEMSCore> logger, IHostApplicationLifetime appLifetime, IWatchdog watchdog, ISmartMeterAdapter smartMeter, IChargePoint chargePoint, ISolar solar, IPriceProvider priceProvider) : base(watchdog)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);
        Logger = logger;

        _smartMeter = smartMeter;
        _chargePoint = chargePoint;
        _solar = solar;
        _priceProvider = priceProvider;

        _compute = new(logger, ChargingMode.MaxCharge);

        appLifetime.ApplicationStarted.Register(OnStarted);
        appLifetime.ApplicationStopping.Register(OnStopping);
        appLifetime.ApplicationStopped.Register(OnStopped);
    }

    protected override void Dispose(bool disposing)
    {
        Logger.LogTrace("Dispose({Disposing}) _disposed {Disposed}", disposing, _disposed);

        if (_disposed) return;

        _solarOptimizerService?.Dispose();
        _solarOptimizerService = null;

        _disposed = true;
        Logger.LogTrace("Dispose({Disposing}) done => _disposed {Disposed}", disposing, _disposed);

        base.Dispose(disposing);
    }


    protected override async Task Start()
    {
        Logger.LogInformation("1. Start has been called.");

        _smartMeter.SmartMeterMeasurementAvailable += SmartMeter_MeasurementAvailable;
        _chargePoint.ChargingStateUpdate += ChargePoint_ChargingStateUpdate;
        _compute.StateUpdate += Compute_StateUpdate;

        _solarOptimizerService = new SolarOptimizer(_priceProvider, _solar, Watchdog);
        await _solarOptimizerService.StartAsync(CancellationToken).ConfigureAwait(false);

        await base.Start().ConfigureAwait(false);
    }

    protected override async void Stop()
    {
        Logger.LogInformation("4. StopAsync has been called.");
        if (_solarOptimizerService is not null)
        {
            await _solarOptimizerService.StopAsync(CancellationToken).ConfigureAwait(false);
            _solarOptimizerService.Dispose();
            _solarOptimizerService = null;
        }
        base.Stop();
    }

    protected override DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(_intervalms);
    }

    protected override int GetInterval()
    {
        return _intervalms;
    }

    protected override Task DoBackgroundWork()
    {
        var (l1, l2, l3) = _compute.Charging();
        try
        {
            _chargePoint.UpdateMaxCurrent(l1, l2, l3);
        }
        catch (CommunicationException ce)
        {
            Logger.LogError("CommunicationException {Message}, while update max current", ce.Message);
        }
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        Logger.LogInformation("2. OnStarted has been called.");
    }

    private void OnStopping()
    {
        Logger.LogInformation("3. OnStopping has been called.");

        _smartMeter.SmartMeterMeasurementAvailable -= SmartMeter_MeasurementAvailable;
        _chargePoint.ChargingStateUpdate -= ChargePoint_ChargingStateUpdate;
        _compute.StateUpdate -= Compute_StateUpdate;
    }

    private void OnStopped()
    {
        Logger.LogInformation("5. OnStopped has been called.");
    }

    private void SmartMeter_MeasurementAvailable(object? sender, SmartMeterMeasurementAvailableEventArgs e)
    {
        Logger.LogTrace("- {Measurement}", e.Measurement);

        var chargePointSocketMeasurement = _chargePoint.LastSocketMeasurement ?? new SocketMeasurementBase();
        _compute.AddMeasurement(e.Measurement, chargePointSocketMeasurement);
    }

    private void ChargePoint_ChargingStateUpdate(object? sender, ChargingStateEventArgs e)
    {
        Logger.LogInformation("- {StateMessage}, {Ended}, {Delivered}, €{Cost} ",
            e.Status?.Measurement?.Mode3StateMessage, e.SessionEnded, e.EnergyDelivered, e.Cost);

        LoggerChargingState.Info("Mode 3 state {state}, {stateMessage}, session ended {ended}, energy delivered {delivered}",
        e.Status?.Measurement?.Mode3State, e.Status?.Measurement?.Mode3StateMessage, e.SessionEnded, e.EnergyDelivered);

        if (e.SessionEnded)
        {
            using (var db = new HEMSContext())
            {

                var energyDelivered = e.EnergyDelivered > 0.0d ? (decimal)e.EnergyDelivered / 1000.0m : 0.01m;
                var sortedCosts = e.Costs.OrderBy((c) => c.Timestamp).ToArray();                

                var transaction = new ChargingTransaction
                {
                    Timestamp = DateTimeProvider.Now,
                    Start = e.Start,
                    End = e.End,
                    EnergyDelivered = (double)energyDelivered,
                    Cost = (double)e.Cost,
                    Price = (double)(e.Cost / energyDelivered)
                };

                LoggerChargingcost.Debug(transaction.ToString());

                foreach (var c in sortedCosts)
                {
                    var energy = c.Energy > 0.0m ? c.Energy / 1000.0m : 0.01m;
                    var detail = new CostDetail()
                    {
                        Timestamp = c.Timestamp,
                        EnergyDelivered = (double)energy,
                    };
                    if (c.Tariff != null)
                    {
                        detail.Cost = (double)(energy * c.Tariff.TariffUsage);
                        detail.TarifStart = c.Tariff.Timestamp;
                        detail.TarifUsage = (double)c.Tariff.TariffUsage;
                    }
                    db.Add(detail);

                    LoggerChargingcost.Debug(detail.ToString());

                    transaction.CostDetails.Add(detail);
                }

                db.Add(transaction);
                db.SaveChanges();
            }
        }
    }

    private void Compute_StateUpdate(object? sender, StateUpdateEventArgs e)
    {
        if (e.Info != null)
        {
            LoggerChargingState.Info($"Mode {e.Info.Mode} - state {e.Info.State} - {e.Info.CurrentAvailableL1} - {e.Info.CurrentAvailableL2} - {e.Info.CurrentAvailableL3}");
        }
        else
        {
            LoggerChargingState.Info($"Mode information not available");
        }
    }
}
