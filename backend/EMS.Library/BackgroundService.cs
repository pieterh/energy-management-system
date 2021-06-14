using System;
using System.Threading;
using System.Threading.Tasks;

namespace EMS.Library
{
    public abstract class BackgroundService : IBackgroundService
    {
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected bool _disposed = false;
 
        private CancellationTokenSource _tokenSource = null;
        public CancellationTokenSource TokenSource { get => _tokenSource; protected set => _tokenSource = value; }

        protected abstract void Start();
        protected abstract void Stop();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Suppress finalization.
        }

        protected virtual void Dispose(bool disposing)
        {
            Logger.Trace($"Dispose({disposing}) _disposed {_disposed}");

            if (_disposed) return;

            if (disposing)
            {
                DisposeTokenSource();
            }

            _disposed = true;
            Logger.Trace($"Dispose({disposing}) done => _disposed {_disposed}");
        }

        private void DisposeTokenSource()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        protected bool StopRequested(int ms)
        {
            if (_tokenSource?.Token == null || _tokenSource.Token.IsCancellationRequested)
                return true;
            if (ms == 0) return false;

            Task.Delay(ms, _tokenSource.Token).GetAwaiter().GetResult();
            return (_tokenSource?.Token == null || _tokenSource.Token.IsCancellationRequested);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Trace($"Starting");
            TokenSource = new CancellationTokenSource();

            Start();            

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            TokenSource?.Cancel();

            Stop();

            return Task.CompletedTask;
        }
    }
}
