using System;
namespace EMS.Library.Adapter
{
    public interface ICurrentMeasurement
    {
        double? CurrentL1 { get; }
        double? CurrentL2 { get; }
        double? CurrentL3 { get; }
    }
}
