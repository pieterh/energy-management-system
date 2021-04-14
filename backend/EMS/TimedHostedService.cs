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
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly double _interval;
        private Timer _timer;

        public TimedHostedService(ILogger<TimedHostedService> logger, IOptions<List<Adapter>> a, IOptions<List<EMS.Library.Configuration.Instance>> i, double interval)
        {
            _logger = logger;
            _interval = interval;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_interval));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
            GC.SuppressFinalize(this);  // Suppress finalization.
        }        
    }
}