using System;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public interface ISmartMeter : IAdapter, IHostedService
    {
        public class MeasurementAvailableEventArgs : EventArgs
        {
            public Measurement Measurement { get; set; }
        }

        public event EventHandler<MeasurementAvailableEventArgs> MeasurementAvailable;
    }
}
