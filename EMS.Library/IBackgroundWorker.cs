using System;
using System.Threading.Tasks;

namespace EMS.Library
{
    public interface IBackgroundWorker : IDisposable
    {
        public Task BackgroundTask { get; }
        public void Start();
        public void Stop();
    }
}