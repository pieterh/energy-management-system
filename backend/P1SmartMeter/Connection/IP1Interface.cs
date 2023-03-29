using System;
using Microsoft.Extensions.Hosting;

namespace P1SmartMeter.Connection
{
    internal interface IP1Reader : IHostedService, IDisposable
    {
        event EventHandler<DataArrivedEventArgs> DataArrived;
    }

    internal sealed class DataArrivedEventArgs : EventArgs
    {
        public string Data { get; set; }
    }
}