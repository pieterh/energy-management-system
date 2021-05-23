using System;
namespace EMS.Library
{
    public enum ChargingMode { MaxCharge, MaxSolar };
    public enum ChargingState { NotCharging, Charging, ChargingPaused }

    public interface IHEMSCore                                              //NOSONAR
    {
        ChargingMode ChargingMode { get; set; }
        ChargeControlInfo ChargeControlInfo { get; }
    }

    public class ChargeControlInfo
    {
        public ChargingMode Mode { get; init; }
        public ChargingState State { get; init; }
        public DateTime LastStateChange { get; init; }
        public double CurrentAvailableL1 { get; init; }
        public double CurrentAvailableL2 { get; init; }
        public double CurrentAvailableL3 { get; init; }

        public ChargeControlInfo()
        {
        }
        public ChargeControlInfo(ChargingMode mode, ChargingState state, DateTime lastStateChange, double l1, double l2, double l3)
        {
            Mode = mode;
            State = state;
            LastStateChange = lastStateChange;
            CurrentAvailableL1 = l1;
            CurrentAvailableL2 = l2;
            CurrentAvailableL3 = l3;
        }
    }
}
