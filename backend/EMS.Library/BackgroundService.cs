using EMS.Library.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EMS.Library
{
    [SuppressMessage("", "S3881")]
    public abstract class BackgroundService : IBackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public bool Disposed { get; protected set; }

        private CancellationToken? _parentToken;
        private CancellationTokenSource _tokenSource = new();
        public CancellationTokenSource TokenSource { get => _tokenSource; protected set => _tokenSource = value; }

        protected abstract Task Start();
        protected abstract void Stop();

        [SuppressMessage("", "CA1063")]
        public void Dispose()
        {
            Logger.Trace("Dispose()");
            Dispose(true);
            GC.SuppressFinalize(this);  // Suppress finalization.
            Disposed = true;
            Logger.Trace("Dispose() done => _disposed {Disposed}", Disposed);
        }

        protected virtual void Dispose(bool disposing)
        {
            Logger.Trace("Dispose({Disposing}) _disposed {Disposed}", disposing, Disposed);

            if (Disposed) return;

            if (disposing)
            {
                DisposeTokenSource();
            }

            Logger.Trace("Dispose({Disposing}) done.", disposing);
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
            if (_parentToken == null) throw new Exceptions.ApplicationException("Missing parent token");
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
