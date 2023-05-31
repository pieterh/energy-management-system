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

        private CancellationToken _parentToken = CancellationToken.None;
        private CancellationTokenSource? _tokenSource = new();
        public CancellationTokenSource? TokenSource { get => _tokenSource ; protected set => _tokenSource = value; }
        public CancellationToken CancellationToken { get => _tokenSource?.Token ?? CancellationToken.None; }

        protected abstract Task Start()
            ;
        [SuppressMessage("", "CA1716")] // no problem with this 'reserved' keyword
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
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        /// <summary>
        /// Delays the task
        /// </summary>
        /// <param name="ms"></param>
        /// <returns>true if a stop was requested, false otherwise</returns>
        public async Task<bool> StopRequested(int ms)
        {
            if (CancellationToken.IsCancellationRequested)
                return true;
            if (ms > 0)
            {
                try
                {
                    await Task.Delay(ms, CancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { /* nothing to do here */  }
            }
            return (CancellationToken.IsCancellationRequested);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Trace($"Starting");

            _parentToken = cancellationToken;
            await StartAsync().ConfigureAwait(false);
        }

        protected Task StartAsync()
        {
            _parentToken.ThrowIfCancellationRequested(); // not starting anymore

            DisposeTokenSource();
            TokenSource = CancellationTokenSource.CreateLinkedTokenSource(_parentToken);

            var task = Start();
            if (task.IsCompleted)
            {
                return task;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // NET 8 has CancelAsync...need to check that out
            TokenSource?.Cancel();

            Stop();

            return Task.CompletedTask;
        }
    }
}
