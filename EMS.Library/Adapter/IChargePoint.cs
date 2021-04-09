using System;
using EMS.Library.Adapter.EVSE;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public class Status
    {
        public bool IsCharging { get; set; }
        public SocketMeasurementBase Measurement { get; set; }
        public Status(SocketMeasurementBase m)
        {
            Measurement = m;
        }
    }

    public interface IChargePoint : IAdapter, IHostedService
    {
        SocketMeasurementBase LastSocketMeasurement { get; }
        void UpdateMaxCurrent(float maxL1, float maxL2, float maxL3);

        public class StatusUpdateEventArgs : EventArgs
        {
            public Status Status { get; set; }

            public StatusUpdateEventArgs(SocketMeasurementBase measurement)
            {
                Status = new Status(measurement);
            }
        }

        public event EventHandler<StatusUpdateEventArgs> StatusUpdate;
    }
}
