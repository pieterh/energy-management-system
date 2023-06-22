using System;
using System.Collections.ObjectModel;

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
        Unknown = 0,
        One = 1,
        Three = 3
    }

    public class SocketMeasurementBase : ICurrentMeasurement
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static Dictionary<Mode3State, string> Mode3States = new Dictionary<Mode3State, string>() {
            { Mode3State.A, "Standby" },
            { Mode3State.B1, "Vehicle detected" },
            { Mode3State.B2, "Vehicle detected (PWM signal applied)" },
            { Mode3State.C1, "Ready Charging" },
            { Mode3State.C2, "Charging (PWM signal applied)" },
            { Mode3State.D1, "Ready with ventilation" },
            { Mode3State.D2, "Charging with ventilation (PWM signal applied)" },
            { Mode3State.E, "No power (shut off)" },
            { Mode3State.F, "Error" }
        };

        public UInt16 MeterState { get; set; }
        public UInt64 MeterTimestamp { get; set; }

        public Mode3State Mode3State { get; set; }
        public string Mode3StateText { get { return Mode3States[Mode3State]; } }

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

        public bool PWMSignalApplied
        {
            get
            {
                if (Mode3State == Mode3State.B2 ||
                    Mode3State == Mode3State.C2 ||
                    Mode3State == Mode3State.D2) return true;
                return false;
            }
        }

        public string Mode3StateMessage
        {
            get
            {
                return Mode3States[Mode3State];
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