﻿using System;
using System.Text;
using Microsoft.Extensions.Hosting;

using EMS.Library.Adapter.EVSE;
using EMS.Library.Core;

namespace EMS.Library
{
    public enum OccpState
    {
        Disconnected = 0,
        Connected = 1
    }

    public class Status
    {
        public bool IsCharging { get { return Measurement.VehicleIsCharging; } }

        public SocketMeasurementBase Measurement { get; set; }
        public Status(SocketMeasurementBase m)
        {
            Measurement = m;
        }
    }

    public record ProductInformation
    {
        public string Name { get; set; } = default!;
        public string Manufacturer { get; set; } = default!;
        public string FirmwareVersion { get; set; } = default!;
        public string Model { get; set; } = default!;
        public string StationSerial { get; set; } = default!;
        public long Uptime { get; set; }
        public DateTime UpSinceUtc { get; set; }
        public DateTime DateTimeUtc { get; set; }

        public virtual StringBuilder ToPrintableString()
        {
            var retval = new StringBuilder();
            retval.AppendFormat("Name                       : {0}", Name);
            retval.AppendFormat("Manufacturer               : {0}", Manufacturer);
            retval.AppendFormat("Firmware version           : {0}", FirmwareVersion);
            retval.AppendFormat("Model                      : {0}", Model);
            retval.AppendFormat("Station serial             : {0}", StationSerial);
            retval.AppendFormat("Uptime                     : {0}", Uptime);
            retval.AppendFormat("Up since                   : {0}", UpSinceUtc.ToString("O"));
            retval.AppendFormat("Date UTC                   : {0}", DateTimeUtc.ToString("O"));
            return retval;
        }
    }

    public record StationStatus
    {
        public float ActiveMaxCurrent { get; set; }
        public float Temperature { get; set; }
        public OccpState OCCPState { get; set; }
        public uint NrOfSockets { get; set; }
    }

    public class ChargingStatusUpdateEventArgs : EventArgs
    {
        public Status Status { get; set; }

        public ChargingStatusUpdateEventArgs(SocketMeasurementBase measurement)
        {
            Status = new Status(measurement);
        }
    }

    public class ChargingStateEventArgs : EventArgs
    {
        public Status Status { get; set; }
        public bool SessionEnded { get; set; }
        public double? EnergyDelivered { get; set; }
        public Decimal Cost { get; set; }
        public IList<Cost> Costs { get; init; }
        public ChargingStateEventArgs(SocketMeasurementBase measurement, bool sessionEnded, double? energyDelivered, Decimal cost, IList<Cost> costs)
        {
            Status = new Status(measurement);
            SessionEnded = sessionEnded;
            EnergyDelivered = sessionEnded ? (energyDelivered ?? 0.0d) : null;
            Cost = cost;
            Costs = costs;
        }
    }

    public interface IChargePoint : IAdapter, IHostedService
    {
        SocketMeasurementBase LastSocketMeasurement { get; }
        ChargeSessionInfoBase ChargeSessionInfo { get; }

        ProductInformation ReadProductInformation();
        StationStatus ReadStationStatus();
        void UpdateMaxCurrent(double maxL1, double maxL2, double maxL3);

        public event EventHandler<ChargingStatusUpdateEventArgs> ChargingStatusUpdate;
        public event EventHandler<ChargingStateEventArgs> ChargingStateUpdate;
    }
}