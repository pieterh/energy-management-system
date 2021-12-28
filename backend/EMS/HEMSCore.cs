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
using EMS.Library.Adapter.SmartMeter;

namespace EMS
{
    public class HEMSCore : Microsoft.Extensions.Hosting.BackgroundService, IHEMSCore, IHostedService    //NOSONAR
    {

        private static readonly NLog.Logger LoggerChargingState = NLog.LogManager.GetLogger("chargingstate");
        private readonly Compute _compute;

        private readonly ILogger Logger;
        private readonly ISmartMeter _smartMeter;
        private readonly IChargePoint _chargePoint;

        private const int _interval = 10000; //ms

        public ChargeControlInfo ChargeControlInfo { get { return _compute.Info; } }

        public ChargingMode ChargingMode {
            get => _compute.Mode;
            set => _compute.Mode = value;
        }

        public HEMSCore(ILogger<HEMSCore> logger, IHostApplicationLifetime appLifetime, ISmartMeter smartMeter, IChargePoint chargePoint)
        {
            Logger = logger;

            _smartMeter = smartMeter;
            _chargePoint = chargePoint;

            _compute = new(logger, ChargingMode.MaxCharge);

            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("1. StartAsync has been called.");
            _smartMeter.MeasurementAvailable += SmartMeter_MeasurementAvailable;
            _chargePoint.ChargingStateUpdate += ChargePoint_ChargingStateUpdate;
            _compute.StateUpdate += Compute_StateUpdate;

            base.StartAsync(cancellationToken);

            return Task.CompletedTask;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ChargingMode = ChargingMode.MaxCharge;

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

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private void OnStarted()
        {
            Logger.LogInformation("2. OnStarted has been called.");
        }

        private void OnStopping()
        {
            Logger.LogInformation("3. OnStopping has been called.");

            _smartMeter.MeasurementAvailable -= SmartMeter_MeasurementAvailable;
            _chargePoint.ChargingStateUpdate -= ChargePoint_ChargingStateUpdate;
            _compute.StateUpdate -= Compute_StateUpdate;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            base.StopAsync(cancellationToken);
            Logger.LogInformation("4. StopAsync has been called.");

            return Task.CompletedTask;
        }

        private void OnStopped()
        {
            Logger.LogInformation("5. OnStopped has been called.");
        }

        private void SmartMeter_MeasurementAvailable(object sender, ISmartMeter.MeasurementAvailableEventArgs e)
        {
            Logger.LogInformation($"- {e.Measurement}");

            _compute.AddMeasurement(e.Measurement, _chargePoint.LastSocketMeasurement);
        }

        private void ChargePoint_ChargingStateUpdate(object sender, IChargePoint.ChargingStateEventArgs e)
        {
            Logger.LogInformation($"- {e.Status?.Measurement?.Mode3StateMessage}, {e.SessionEnded}, {e.EnergyDelivered} ");
            LoggerChargingState.Info($"Mode 3 state {e.Status?.Measurement?.Mode3State}, {e.Status?.Measurement?.Mode3StateMessage}, session ended {e.SessionEnded}, energy delivered {e.EnergyDelivered}");
        }

        private void Compute_StateUpdate(object sender, Compute.StateEventArgs e)
        {
            LoggerChargingState.Info($"Mode {e.Info.Mode} - state {e.Info.State} - {e.Info.CurrentAvailableL1} - {e.Info.CurrentAvailableL2} - {e.Info.CurrentAvailableL3}");
        }
    }
}
