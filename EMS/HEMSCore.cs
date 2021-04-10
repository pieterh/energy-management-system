using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EMS.Library;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using EMS.Library.Configuration;
using System;
using System.Linq;
using EMS.Library.Adapter.EVSE;

namespace EMS
{

    public interface IHEMSCore
    {

    }

    public class HEMSCore : BackgroundService, IHEMSCore, IHostedService
    {
        private readonly Compute c = new (Compute.ChargingMode.MaxCharge);

        private readonly ILogger Logger;
        private readonly ISmartMeter _smartMeter;
        private readonly IChargePoint _chargePoint;

        private const int _interval = 10000; //ms

        public HEMSCore(ILogger<HEMSCore> logger, IHostApplicationLifetime appLifetime, ISmartMeter smartMeter, IChargePoint chargePoint)
        {
            Logger = logger;

            _smartMeter = smartMeter;
            _chargePoint = chargePoint;

            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            base.StartAsync(cancellationToken);
            Logger.LogInformation("1. StartAsync has been called.");

            return Task.CompletedTask;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //c.Mode = Compute.ChargingMode.MaxSolar;

            while (!stoppingToken.IsCancellationRequested)
            {
                var measurememt = _chargePoint.LastSocketMeasurement;

                var ci = new ChargingInfo(
                    measurememt.CurrentL1, measurememt.CurrentL2, measurememt.CurrentL3,
                    measurememt.VoltageL1, measurememt.VoltageL2, measurememt.VoltageL3);                

                (float l1, float l2, float l3) = c.Charging(Logger, ci);

                try
                {
                    _chargePoint.UpdateMaxCurrent( l1,  l2,  l3);
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

            _smartMeter.MeasurementAvailable += SmartMeter_MeasurementAvailable;
            _chargePoint.ChargingStateUpdate += ChargePoint_ChargingStateUpdate;
        }

        private void OnStopping()
        {
            Logger.LogInformation("3. OnStopping has been called.");

            _smartMeter.MeasurementAvailable -= SmartMeter_MeasurementAvailable;
            _chargePoint.ChargingStateUpdate -= ChargePoint_ChargingStateUpdate;
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
            c.AddMeasurement(e.Measurement);
        }

        private void ChargePoint_ChargingStateUpdate(object sender, IChargePoint.ChargingStateEventArgs e)
        {
            Logger.LogInformation($"- {e.Status?.Measurement?.Mode3StateMessage}");
        }
    }
}
