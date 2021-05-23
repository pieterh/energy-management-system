using System;

namespace EMS.Library
{
    public class MeasurementBase
    {
        public DateTime Received { get; set; }

        public DateTime? Timestamp { get; protected set; }

        public double? CurrentL1 { get; protected set; }
        public double? CurrentL2 { get; protected set; }
        public double? CurrentL3 { get; protected set; }

        public double? VoltageL1 { get; protected set; }
        public double? VoltageL2 { get; protected set; }
        public double? VoltageL3 { get; protected set; }

        public int? TariffIndicator { get; protected set; }

        public double? Electricity1FromGrid { get; protected set; }

        public double? Electricity2FromGrid { get; protected set; }

        public double? Electricity1ToGrid { get; protected set; }

        public double? Electricity2ToGrid { get; protected set; }


        public MeasurementBase() { }

        public MeasurementBase(double? c1, double? c2, double? c3, double? v1, double? v2, double? v3)
            : this(DateTime.Now, c1, c2, c3, v1, v2, v3)
        {            
        }

        public MeasurementBase(DateTime received, double? c1, double? c2, double? c3, double? v1, double? v2, double? v3)
        {
            Received = received;

            CurrentL1 = c1;
            CurrentL2 = c2;
            CurrentL3 = c3;

            VoltageL1 = v1;
            VoltageL2 = v2;
            VoltageL3 = v3;
        }

        public override string ToString()
        {
            return $"{Timestamp}; new MeasurementBase({CurrentL1}, {CurrentL2}, {CurrentL3}, {VoltageL1}, {VoltageL2}, {VoltageL3})";
        }
    }

    public class Measurement : MeasurementBase
    {

    }
}
