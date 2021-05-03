using System;
namespace EMS.Library.Adapter.EVSE
{
    public class ChargeSessionInfoBase
    {
        public Nullable<DateTime> Start { get; set; }
        public Nullable<DateTime> End { get; set; }
        public UInt32 ChargingTime { get; set; }
        public double EnergyDelivered { get; set; }
        public bool SessionEnded { get; set; }
    }
}
