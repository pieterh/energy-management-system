using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EMS.Library.Configuration;

namespace EMS
{
    public class TimedHostedService2 : BackgroundService
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly double _interval;

        public TimedHostedService2(ILogger<TimedHostedService> logger, IOptions<List<Adapter>> a, IOptions<List<EMS.Library.Configuration.Instance>> i, double interval)
        {
            _logger = logger;
            _interval = interval;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var count = Interlocked.Increment(ref executionCount);

                _logger.LogInformation("Timed Hosted Service as a BackgroundService is working. Count: {Count}", count);

                await Task.Delay((int)_interval, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}