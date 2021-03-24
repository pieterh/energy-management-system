using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EMS.Library;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using EMS.Library.Configuration;

namespace EMS
{
    public interface IHEMSCore
    {

    }

    public class HEMSCore : IHEMSCore, IHostedService
    {
        private readonly ILogger Logger;

        public HEMSCore(ILogger<HEMSCore> logger, IHostApplicationLifetime appLifetime, ISmartMeter smartMeter, IChargePoint chargePoint)
        {
            Logger = logger;

            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("1. StartAsync has been called.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("4. StopAsync has been called.");

            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            Logger.LogInformation("2. OnStarted has been called.");
        }

        private void OnStopping()
        {
            Logger.LogInformation("3. OnStopping has been called.");
        }

        private void OnStopped()
        {
             Logger.LogInformation("5. OnStopped has been called.");
        }
    }
}
