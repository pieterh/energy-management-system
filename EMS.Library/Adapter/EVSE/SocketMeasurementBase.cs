using System;
namespace EMS.Library.Adapter.EVSE
{
    public enum Mode3State
    {
        A = 0,
        B1 = 1,
        B2 = 2,
        C1 = 3,
        C2 = 4,
        D1 = 5,
        D2 = 6,
        E = 7,
        F = 8,
        UnknownState = -1
    }
    public enum Phases
    {
        One = 1,
        Three = 3,
        Unknown = -1
    }
    public class SocketMeasurementBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public UInt16 MeterState { get; set; }
        public UInt64 MeterTimestamp { get; set; }

        public Mode3State Mode3State { get; set; }
        public float VoltageL1 { get; set; }
        public float VoltageL2 { get; set; }
        public float VoltageL3 { get; set; }
        public float CurrentL1 { get; set; }
        public float CurrentL2 { get; set; }
        public float CurrentL3 { get; set; }

        public float AppliedMaxCurrent { get; set; }

        public UInt32 MaxCurrentValidTime { get; set; }
        public float MaxCurrent { get; set; }
        public float ActiveLBSafeCurrent { get; set; }
        public bool SetPointAccountedFor { get; set; }
        public Phases Phases { get; set; }

        public static Mode3State ParseMode3State(string mode3State)
        {
            if (Enum.TryParse(typeof(Mode3State), mode3State, out object mode3StateEnum))
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