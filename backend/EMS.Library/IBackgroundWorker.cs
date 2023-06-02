using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public interface IBackgroundWorker 
    {
        public Task? BackgroundTask { get; }
        public Task Restart();
    }
}