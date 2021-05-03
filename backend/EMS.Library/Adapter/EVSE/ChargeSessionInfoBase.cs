using System;
namespace EMS.Library.Adapter.EVSE
{
    public class ChargeSessionInfoBase
    {
        public DateTime Start { get; set; }
        public UInt32 ChargingTime { get; set; }
        public double EnergyDelivered { get; set; }
    }
}
