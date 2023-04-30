namespace EMS.Library.Shared.DTO.HEMS;

public class Measurement
{
    public DateTime Received { get; init; }
    public double L1 { get; init; }
    public double L2 { get; init; }
    public double L3 { get; init; }
    public double L { get; init; }

    public double CL1 { get; init; }
    public double CL2 { get; init; }
    public double CL3 { get; init; }
}