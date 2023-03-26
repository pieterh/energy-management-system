using EMS.Library.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EMS.Library
{
    public abstract class BackgroundService : IBackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public bool Disposed { get; protected set; }

        private CancellationToken? _parentToken;
        private CancellationTokenSource _tokenSource = new();
        public CancellationTokenSource TokenSource { get => _tokenSource; protected set => _tokenSource = value; }


        protected abstract Task Start();
        protected abstract void Stop();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Suppress finalization.
        }

        protected virtual void Dispose(bool disposing)
        {
            Logger.Trace($"Dispose({disposing}) _disposed {Disposed}");

            if (Disposed) return;

            if (disposing)
            {
                DisposeTokenSource();
            }

            Disposed = true;
            Logger.Trace($"Dispose({disposing}) done => _disposed {Disposed}");
        }

        private void DisposeTokenSource()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        public async Task<bool> StopRequested(int ms)
        {
            if (_tokenSource.Token.IsCancellationRequested)
                return true;
            if (ms > 0)
            {
                try
                {
                    await Task.Delay(ms, _tokenSource.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { /* nothing to do here */  }
            }
            return (_tokenSource.Token.IsCancellationRequested);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Trace($"Starting");

            _parentToken = cancellationToken;
            await StartAsync().ConfigureAwait(false);
        }
        protected Task StartAsync()
        {
            if (_parentToken == null) throw new HEMSApplicationException("Missing parent token");
            _parentToken.Value.ThrowIfCancellationRequested(); // not starting anymore

            DisposeTokenSource();
            TokenSource = CancellationTokenSource.CreateLinkedTokenSource(_parentToken.Value);

            var task = Start();
            if (task.IsCompleted)
            {
                return task;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            TokenSource.Cancel();

            Stop();

            return Task.CompletedTask;
        }
    }
}
