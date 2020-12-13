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
    public class SocketMeasurement
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public UInt16 MeterState { get; set; }
        public UInt64 MeterTimestamp { get; set; }
        public MeterType MeterType { get; set; }



        public double RealEnergyDeliveredL1 { get; set; }
        public double RealEnergyDeliveredL2 { get; set; }
        public double RealEnergyDeliveredL3 { get; set; }
        public double RealEnergyDeliveredSum { get; set; }

        public bool Availability { get; set; }
        public Mode3State Mode3State { get; set; }

        public float AppliedMaxCurrent { get; set; }

        public UInt32 MaxCurrentValidTime { get; set; }
        public float MaxCurrent { get; set; }
        public float ActiveLBSafeCurrent { get; set; }
        public bool SetPointAccountedFor { get; set; }
        public Phases Phases { get; set; }

        public static MeterType ParseMeterType(ushort meterType)
        {
            if (Enum.IsDefined(typeof(MeterType), (System.Int32)meterType))
            {
                return (MeterType)meterType;
            }

            Logger.Error($"Unknown meter type received -> {meterType}");
            return MeterType.UnknownType;
        }

        public static Mode3State ParseMode3State(string mode3State)
        {
            Object mode3StateEnum;
            if (Enum.TryParse(typeof(Mode3State), mode3State, out mode3StateEnum))
            {
                return (Mode3State)mode3StateEnum;
            }

            Logger.Error($"Unknown Mode 3 state received -> {mode3State}");
            return Mode3State.UnknownState;
        }

        public static Phases ParsePhases(ushort phases)
        {
            if (Enum.IsDefined(typeof(Phases), (System.Int32)phases))
            {
                return (Phases)phases;
            }

            Logger.Error($"Unknown number of phases received -> {phases}");
            return Phases.Unknown;
        }
    }
}
