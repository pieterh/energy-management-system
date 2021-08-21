using System;
namespace EMS.Library.Core
{
    public enum ChargingMode { MaxCharge, MaxEco, MaxSolar, SlowCharge };
    public enum ChargingState { NotCharging, Charging, ChargingPaused }

    public interface IHEMSCore                                              //NOSONAR
    {
        ChargingMode ChargingMode { get; set; }
        ChargeControlInfo ChargeControlInfo { get; }
    }
}
