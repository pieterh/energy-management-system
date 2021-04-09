using System;

namespace EMS.Library
{
    public class MeasurementBase
    {
        public DateTime Received { get; set; }
        public DateTime? Timestamp { get; protected set; }
        public int? CurrentL1 { get; protected set; }
        public int? CurrentL2 { get; protected set; }
        public int? CurrentL3 { get; protected set; }

        public double? PowerUsingL1 { get; protected set; }         // kW
        public double? PowerUsingL2 { get; protected set; }         // kW
        public double? PowerUsingL3 { get; protected set; }         // kW

        public double? PowerReturningL1 { get; protected set; }     // kW
        public double? PowerReturningL2 { get; protected set; }     // kW
        public double? PowerReturningL3 { get; protected set; }     // kW

        public double? PowerL1 { get { return PowerUsingL1.HasValue && PowerUsingL1.Value > 0 ? -PowerUsingL1 : PowerReturningL1.HasValue ? PowerReturningL1 : null; } }
        public double? PowerL2 { get { return PowerUsingL2.HasValue && PowerUsingL2.Value > 0 ? -PowerUsingL2 : PowerReturningL2.HasValue ? PowerReturningL2 : null; } }
        public double? PowerL3 { get { return PowerUsingL3.HasValue && PowerUsingL3.Value > 0 ? -PowerUsingL3 : PowerReturningL3.HasValue ? PowerReturningL3 : null; } }

        public MeasurementBase() { }

        public MeasurementBase(int? c1, int? c2, int? c3, double? ul1, double? ul2, double? ul3, double? rl1, double? rl2, double? rl3)
            : this(DateTime.Now, c1, c2, c3, ul1, ul2, ul3, rl1, rl2, rl3)
        {
            
        }

        public MeasurementBase(DateTime received, int? c1, int? c2, int? c3, double? ul1, double? ul2, double? ul3, double? rl1, double? rl2, double? rl3)
        {
            Received = received;

            CurrentL1 = c1;
            CurrentL2 = c2;
            CurrentL3 = c3;

            PowerUsingL1 = ul1;
            PowerUsingL2 = ul2;
            PowerUsingL3 = ul3;

            PowerReturningL1 = rl1;
            PowerReturningL2 = rl2;
            PowerReturningL3 = rl3;
        }

        public override string ToString()
        {
            return $"{Timestamp}; new MeasurementBase({CurrentL1}, {CurrentL2}, {CurrentL3}, {PowerUsingL1}, {PowerUsingL2}, {PowerUsingL3}, {PowerReturningL1}, {PowerReturningL2}, {PowerReturningL3})";
        }
    }

    public class Measurement : MeasurementBase
    {
        public int? TariffIndicator { get; protected set; }

        public double? Electricity1FromGrid { get; protected set; }

        public double? Electricity2FromGrid { get; protected set; }

        public double? Electricity1ToGrid { get; protected set; }

        public double? Electricity2ToGrid { get; protected set; }
    }
}
