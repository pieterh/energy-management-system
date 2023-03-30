using System;
using Microsoft.Extensions.Hosting;

namespace EMS.Library.Adapter.SmartMeterAdapter
{
    public class SmartMeterMeasurementAvailableEventArgs : EventArgs
    {
        public required SmartMeterMeasurement Measurement { get; init; }
    }

    public interface ISmartMeterAdapter : IAdapter, IHostedService
    {
        public SmartMeterMeasurementBase LastMeasurement { get; }


        public event EventHandler<SmartMeterMeasurementAvailableEventArgs> SmartMeterMeasurementAvailable;
    }
}
