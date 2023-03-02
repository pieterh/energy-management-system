using System;
using System.Collections.Generic;

namespace EMS.Library.Core
{
    public record ChargeControlInfo
    {
        public ChargingMode Mode { get; init; }
        public ChargingState State { get; init; }
        public DateTime LastStateChange { get; init; }
        public double CurrentAvailableL1 { get; init; }
        public double CurrentAvailableL2 { get; init; }
        public double CurrentAvailableL3 { get; init; }
        public IEnumerable<Measurement> Measurements { get; init;}

        public ChargeControlInfo()
        {
            Measurements = new List<Measurement>();
        }

        public ChargeControlInfo(ChargingMode mode, ChargingState state, DateTime lastStateChange, double l1, double l2, double l3, IEnumerable<Measurement> m)
        {
            Mode = mode;
            State = state;
            LastStateChange = lastStateChange;
            CurrentAvailableL1 = l1;
            CurrentAvailableL2 = l2;
            CurrentAvailableL3 = l3;
            Measurements = m;
        }
    }
}
