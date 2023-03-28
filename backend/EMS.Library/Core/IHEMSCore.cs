using System;
using System.Diagnostics.CodeAnalysis;

namespace EMS.Library.Core
{
    public enum ChargingMode { MaxCharge, MaxEco, MaxSolar, SlowCharge };
    public enum ChargingState { NotCharging, Charging, ChargingPaused }
    [SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
    public interface IHEMSCore
    {
        ChargingMode ChargingMode { get; set; }
        ChargeControlInfo ChargeControlInfo { get; }
    }
}
