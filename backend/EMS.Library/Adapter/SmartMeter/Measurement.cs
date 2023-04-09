using System;
using EMS.Library.Adapter;
using EMS.Library.TestableDateTime;
namespace EMS.Library.Adapter.SmartMeterAdapter
{
    public record SmartMeterMeasurementBase : ICurrentMeasurement
    {
        public DateTime Received { get; init; }

        public DateTime? Timestamp { get; init; }

        public int? TariffIndicator { get; init; }

        public double? CurrentL1 { get; init; }
        public double? CurrentL2 { get; init; }
        public double? CurrentL3 { get; init; }

        public double? VoltageL1 { get; init; }
        public double? VoltageL2 { get; init; }
        public double? VoltageL3 { get; init; }

        public double? Electricity1FromGrid { get; init; }

        public double? Electricity2FromGrid { get; init; }

        public double? Electricity1ToGrid { get; init; }

        public double? Electricity2ToGrid { get; init; }

        public override string ToString()
        {
            return $"{Timestamp}; new MeasurementBase({CurrentL1}, {CurrentL2}, {CurrentL3}, {VoltageL1}, {VoltageL2}, {VoltageL3})";
        }
    }
}
