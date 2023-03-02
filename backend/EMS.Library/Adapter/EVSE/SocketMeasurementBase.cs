using System;
namespace EMS.Library.Adapter.EVSE
{
    public enum Mode3State
    {
        A = 0,      // Standby
        B1 = 1,     // Vehicle detected
        B2 = 2,     // Vehicle detected (PWM signal applied)
        C1 = 3,     // Ready Charging
        C2 = 4,     // Charging (PWM signal applied)
        D1 = 5,     // Ready with ventilation
        D2 = 6,     // Charging with ventilation (PWM signal applied)
        E = 7,      // No power (shut off)
        F = 8,      // Error
        UnknownState = -1
    }

    public enum Phases
    {
        One = 1,
        Three = 3,
        Unknown = -1
    }

    public class SocketMeasurementBase : ICurrentMeasurement
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public UInt16 MeterState { get; set; }
        public UInt64 MeterTimestamp { get; set; }

        public Mode3State Mode3State { get; set; }
        public DateTime LastChargingStateChanged { get; set; }

        public float Voltage { get; set; }
        public float VoltageL1 { get; set; }
        public float VoltageL2 { get; set; }
        public float VoltageL3 { get; set; }

        public double? CurrentL1 { get; set; }
        public double? CurrentL2 { get; set; }
        public double? CurrentL3 { get; set; }

        public double CurrentSum { get; set; }

        public float RealPowerSum { get; set; }

        public double RealEnergyDeliveredSum { get; set; }

        public float AppliedMaxCurrent { get; set; }

        public bool Availability { get; set; }
        public UInt32 MaxCurrentValidTime { get; set; }
        public float MaxCurrent { get; set; }
        public float ActiveLBSafeCurrent { get; set; }
        public bool SetPointAccountedFor { get; set; }
        public Phases Phases { get; set; }

        public bool VehicleConnected
        {
            get
            {
                if (Mode3State == Mode3State.A ||
                    Mode3State == Mode3State.E ||
                    Mode3State == Mode3State.F) return false;
                return true;
            }
        }

        public bool VehicleIsCharging
        {
            get
            {
                if (Mode3State == Mode3State.C2 ||
                    Mode3State == Mode3State.D2) return true;
                return false;
            }
        }

        public string Mode3StateMessage { get {
                switch (Mode3State)
                {
                    case Mode3State.A: return "Standby";
                    case Mode3State.B1: return "Vehicle detected";
                    case Mode3State.B2: return "Vehicle detected";
                    case Mode3State.C1: return "Ready charging";
                    case Mode3State.C2: return "Charging";
                    case Mode3State.D1: return "Ready charging in ventilated area";
                    case Mode3State.D2: return "Charging in ventilated area";
                    case Mode3State.E: return "No Power";
                    case Mode3State.F: return "Error";
                    default: return "Unknown state";
                }
            }
        }

        public static Mode3State ParseMode3State(string mode3State)
        {
            if (Enum.TryParse(typeof(Mode3State), mode3State, out object? mode3StateEnum))
            {
                return (Mode3State)mode3StateEnum;
            }

            Logger.Error($"Unknown Mode 3 state received -> {mode3State}");
            return Mode3State.UnknownState;
        }

        public static Phases ParsePhases(ushort phases)
        {
            if (Enum.IsDefined(typeof(Phases), (Int32)phases))
            {
                return (Phases)phases;
            }

            Logger.Error($"Unknown number of phases received -> {phases}");
            return Phases.Unknown;
        }

        public override string ToString()
        {
            return $"Meter state: {MeterState}; Safe current: {ActiveLBSafeCurrent}A; State: {Mode3State}; Phases: {(Int16)Phases}; Max: {MaxCurrent}A; Applied: {AppliedMaxCurrent}A; Valid: {MaxCurrentValidTime}S";
        }
    }


}