using System;
using Microsoft.Extensions.Hosting;

namespace EMS.Library.Adapter.SmartMeter
{
    public interface ISmartMeter : IAdapter, IHostedService
    {
        public SmartMeterMeasurementBase LastMeasurement { get; }
        public class MeasurementAvailableEventArgs : EventArgs
        {
            public required SmartMeterMeasurement Measurement { get; init; }
        }

        public event EventHandler<MeasurementAvailableEventArgs> MeasurementAvailable;
    }
}
