using System;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter.Reading
{
    public class Measurement
    {
        public DateTime? Timestamp { get; }

        //public string TextMessage { get; }

        //public double? ActualPower { get; }

        public int? TariffIndicator { get; }

        //public double? Electricity1FromGrid { get; }

        //public double? Electricity2FromGrid { get; }

        //public double? Electricity1ToGrid { get; }

        //public double? Electricity2ToGrid { get; }

        public double? PowerUsingL1 { get; }
        public double? PowerUsingL2 { get; }
        public double? PowerUsingL3 { get; }

        public double? PowerReturningL1 { get; }
        public double? PowerReturningL2 { get; }
        public double? PowerReturningL3 { get; }

        public Measurement(DSMRTelegram t)
        {
            Timestamp = t.Timestamp;
            TariffIndicator = t.TariffIndicator;
            PowerUsingL1 = t.PowerUsedL1;
            PowerUsingL2 = t.PowerUsedL2;
            PowerUsingL3 = t.PowerUsedL3;
            PowerReturningL1 = t.PowerReturnedL1;
            PowerReturningL2 = t.PowerReturnedL2;
            PowerReturningL3 = t.PowerReturnedL3;
        }

        public override string ToString()
        {
            return $"{Timestamp}; UL1: {PowerUsingL1}; UL2: {PowerUsingL2}; UL3: {PowerUsingL3}; RL1: {PowerReturningL1}; RL2: {PowerReturningL2}; RL3: {PowerReturningL3}; T: {TariffIndicator}";
        }
    }
}
