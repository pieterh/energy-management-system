using System;
using EMS.Library.Adapter.EVSE;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public class Status
    {
        public bool IsCharging { get { return Measurement.VehicleIsCharging; } }

        public SocketMeasurementBase Measurement { get; set; }
        public Status(SocketMeasurementBase m)
        {
            Measurement = m;
        }
    }

    public interface IChargePoint : IAdapter, IHostedService
    {
        SocketMeasurementBase LastSocketMeasurement { get; }

        void UpdateMaxCurrent(double maxL1, double maxL2, double maxL3);

        public class StatusUpdateEventArgs : EventArgs
        {
            public Status Status { get; set; }

            public StatusUpdateEventArgs(SocketMeasurementBase measurement)
            {
                Status = new Status(measurement);
            }
        }

        public event EventHandler<StatusUpdateEventArgs> StatusUpdate;

        public class ChargingStateEventArgs : EventArgs
        {
            public Status Status { get; set; }
            public bool SessionEnded { get; set; }
            public double? EnergyDelivered { get; set; }
            public ChargingStateEventArgs(SocketMeasurementBase measurement, bool sessionEnded, double? energyDelivered)
            {
                Status = new Status(measurement);
                SessionEnded = sessionEnded;
                EnergyDelivered = sessionEnded ? energyDelivered.Value : null;
            }
        }

        public event EventHandler<ChargingStateEventArgs> ChargingStateUpdate;
    }
}
