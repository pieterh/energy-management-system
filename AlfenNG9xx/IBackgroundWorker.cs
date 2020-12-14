using System;
using System.Threading.Tasks;

namespace AlfenNG9xx
{
    public interface IBackgroundWorker : IDisposable
    {
        public Task BackgroundTask { get; }
        public void Start();
        public void Stop();
    }
}