using System;
using System.Diagnostics.CodeAnalysis;
using EMS.Library.Adapter.EVSE;

namespace AlfenNG9xx.Model
{
    [SuppressMessage("Code Analysis", "CA1707")]
    public enum MeterType
    {
        RTU = 0,
        TCP_IP = 1,
        UDP = 2,
        P1 = 3,
        Other = 4,
        UnknownType = -1
    }

    public class SocketMeasurement : SocketMeasurementBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MeterType MeterType { get; set; }

        public float RealPowerL1 { get; set; }
        public float RealPowerL2 { get; set; }
        public float RealPowerL3 { get; set; }

        public double RealEnergyDeliveredL1 { get; set; }
        public double RealEnergyDeliveredL2 { get; set; }
        public double RealEnergyDeliveredL3 { get; set; }
        
        public static MeterType ParseMeterType(ushort meterType)
        {
            if (Enum.IsDefined(typeof(MeterType), (Int32)meterType))
            {
                return (MeterType)meterType;
            }

            Logger.Error($"Unknown meter type received -> {meterType}");
            return MeterType.UnknownType;
        }

        public override string ToString()
        {
            return $"Meter state: {MeterState}; Available: {Availability}; Safe current: {ActiveLBSafeCurrent}A; State: {Mode3State}; Phases: {(Int16)Phases}; Max: {MaxCurrent}A; Applied: {AppliedMaxCurrent}A; Valid: {MaxCurrentValidTime}S";
        }
    }
}
