using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Engine;
using EMS.Library.Core;
using EMS.Library.Adapter.SmartMeterAdapter;
using EMS.Library.TestableDateTime;
using EMS.DataStore;
using System.Diagnostics.CodeAnalysis;
using EMS.Library.Adapter.Solar;
using EMS.Library.Adapter.PriceProvider;

namespace EMS
{
    [SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
    public class HEMSCore : Microsoft.Extensions.Hosting.BackgroundService, IHEMSCore, IHostedService
    {
        private static readonly NLog.Logger LoggerChargingState = NLog.LogManager.GetLogger("chargingstate");
        private static readonly NLog.Logger LoggerChargingcost = NLog.LogManager.GetLogger("chargingcost");
        private readonly Compute _compute;

        private readonly ILogger Logger;
        private readonly ISmartMeterAdapter _smartMeter;
        private readonly IChargePoint _chargePoint;
        private readonly ISolar _solar;
        private readonly IPriceProvider _priceProvider;

        private const int _interval = 10000; //ms

        public ChargeControlInfo ChargeControlInfo { get { return _compute.Info; } }

        private bool _disposed;
        private SolarOptimizer? _solarOptimizerService;

        public ChargingMode ChargingMode
        {
            get => _compute.Mode;
            set => _compute.Mode = value;
        }

        public HEMSCore(ILogger<HEMSCore> logger, IHostApplicationLifetime appLifetime, ISmartMeterAdapter smartMeter, IChargePoint chargePoint, ISolar solar, IPriceProvider priceProvider)
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

        public override void Dispose()
        {
            Dispose(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        // Unfortunately the base class doesn't implement the dispose pattern
        // properly, so there is no method Dispose(bool disposing) to override...
        // https://github.com/dotnet/runtime/issues/34809
        // Therefor we need to suppress the sonar message
        [SuppressMessage("Sonar", "S2953")]
        internal virtual void Dispose(bool disposing)
        {
            Logger.LogTrace("Dispose({Disposing}) _disposed {Disposed}", disposing, _disposed);

            if (_disposed) return;

            _solarOptimizerService?.Dispose();
            _solarOptimizerService = null;

            _disposed = true;
            Logger.LogTrace("Dispose({Disposing}) done => _disposed {Disposed}", disposing, _disposed);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("1. StartAsync has been called.");
            _smartMeter.SmartMeterMeasurementAvailable += SmartMeter_MeasurementAvailable;
            _chargePoint.ChargingStateUpdate += ChargePoint_ChargingStateUpdate;
            _compute.StateUpdate += Compute_StateUpdate;

            _solarOptimizerService = new SolarOptimizer(_priceProvider, _solar);
            await _solarOptimizerService.StartAsync(cancellationToken).ConfigureAwait(false);

            await base.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        [SuppressMessage("Code Analysis", "CA1031")]
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                ChargingMode = ChargingMode.MaxCharge;

                using (var db = new HEMSContext())
                {

                    Logger.LogInformation("Database path: {Path}.", HEMSContext.DbPath);

                    var items = db.ChargingTransactions.OrderBy((x) => x.Timestamp);
                    foreach (var item in items)
                    {
                        Logger.LogTrace("Transaction: {Trans}", item.ToString());
                        db.Entry(item)
                            .Collection(b => b.CostDetails)
                            .Load();
                        foreach (var detail in item.CostDetails.OrderBy((x) => x.Timestamp))
                        {
                            Logger.LogTrace("Transaction detail: {Detail}", detail.ToString());
                        }
                    }
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    var (l1, l2, l3) = _compute.Charging();

                    try
                    {
                        _chargePoint.UpdateMaxCurrent(l1, l2, l3);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "while update max current");
                    }

                    await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException tce)
            {
                Logger.LogInformation("Canceled");
                Logger.LogError(tce, "did we expect this?");
                Logger.LogError("did we expect this? {IsCancellationRequested}", stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled exception");
            }
            
            stoppingToken.ThrowIfCancellationRequested();
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken).ConfigureAwait(false);

            Logger.LogInformation("4. StopAsync has been called.");
            if (_solarOptimizerService is not null)
            {
                await _solarOptimizerService.StopAsync(cancellationToken).ConfigureAwait(false);
                _solarOptimizerService.Dispose();
                _solarOptimizerService = null;
            }
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

                    var transaction = new ChargingTransaction
                    {
                        Timestamp = DateTimeProvider.Now,
                        EnergyDelivered = (double)energyDelivered,
                        Cost = (double)e.Cost,
                        Price = (double)(e.Cost / energyDelivered)
                    };

                    LoggerChargingcost.Debug(transaction.ToString());

                    foreach (var c in e.Costs)
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
}
