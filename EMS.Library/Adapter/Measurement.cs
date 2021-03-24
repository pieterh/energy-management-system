using System;

namespace EMS.Library
{
    public class Measurement
    {
        public DateTime? Timestamp { get; protected set; }

        public int? TariffIndicator { get; protected set; }

        public double? Electricity1FromGrid { get; protected set; }

        public double? Electricity2FromGrid { get; protected set; }

        public double? Electricity1ToGrid { get; protected set; }

        public double? Electricity2ToGrid { get; protected set; }

        public double? PowerUsingL1 { get; protected set; }
        public double? PowerUsingL2 { get; protected set; }
        public double? PowerUsingL3 { get; protected set; }

        public double? PowerReturningL1 { get; protected set; }
        public double? PowerReturningL2 { get; protected set; }
        public double? PowerReturningL3 { get; protected set; }

        public override string ToString()
        {
            return $"{Timestamp}; UL1: {PowerUsingL1}; UL2: {PowerUsingL2}; UL3: {PowerUsingL3}; RL1: {PowerReturningL1}; RL2: {PowerReturningL2}; RL3: {PowerReturningL3}; T: {TariffIndicator}";
        }
    }
}
