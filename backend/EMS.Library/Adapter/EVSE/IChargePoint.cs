using System;
using System.Collections.Generic;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Core;
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
        ChargeSessionInfoBase ChargeSessionInfo { get; }

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
            public Decimal Cost { get; set; }
            public IList<Cost> Costs { get; set; }
            public ChargingStateEventArgs(SocketMeasurementBase measurement, bool sessionEnded, double? energyDelivered, Decimal cost, IList<Cost> costs)
            {
                Status = new Status(measurement);
                SessionEnded = sessionEnded;
                EnergyDelivered = sessionEnded ? (energyDelivered ?? 0.0d) : null;
                Cost = cost;
                Costs = costs;
            }
        }

        public event EventHandler<ChargingStateEventArgs> ChargingStateUpdate;
    }
}
