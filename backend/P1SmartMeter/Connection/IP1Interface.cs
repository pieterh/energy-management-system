using System;
using Microsoft.Extensions.Hosting;

namespace P1SmartMeter.Connection
{
    public interface IP1Interface: IHostedService, IDisposable                              //NOSONAR
    {
        event EventHandler<DataArrivedEventArgs> DataArrived;

        public class DataArrivedEventArgs : EventArgs
        {
            public string Data { get; set; }
        }
    }
}
