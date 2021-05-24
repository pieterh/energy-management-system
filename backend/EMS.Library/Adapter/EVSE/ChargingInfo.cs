using System;
namespace EMS.Library.Adapter.EVSE
{
    public class ChargingInfo
    {
        public float CurrentL1 { get; set; }
        public float CurrentL2 { get; set; }
        public float CurrentL3 { get; set; }

        public ChargingInfo(float c1, float c2, float c3)
        {
            CurrentL1 = c1;
            CurrentL2 = c2;
            CurrentL3 = c3;
        }
    }
}
