using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public interface IBackgroundWorker : IHostedService, IDisposable
    {
        public Task BackgroundTask { get; }
    }
}