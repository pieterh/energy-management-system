using System;

namespace AlfenNG9xx.Model
{
    public enum MeterType
    {
        RTU = 0,
        TCP_IP = 1,
        UDP = 2,
        P1 = 3,
        Other = 4,
        UnknownType = -1
    }
    public class SocketMeasurement
    {
        public UInt16 MeterState { get; set; }
        public UInt64 MeterTimestamp { get; set; }
        public MeterType MeterType { get; set; }

        public bool Availability { get; set; }
        public string Mode3State { get; set; }

        public double RealEnergyDeliveredL1 { get; set; }
        public double RealEnergyDeliveredL2 { get; set; }
        public double RealEnergyDeliveredL3 { get; set; }
        public double RealEnergyDeliveredSum { get; set; }

        
        public UInt32 MaxCurrentValidTime { get; set; }
        public float MaxCurrent { get; set; }        
    }
}
