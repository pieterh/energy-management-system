namespace EMS.Library.Shared.DTO.SmartMeter;

public class SmartMeterInfoModel
{
    public required string CurrentL1 { get; init; }
    public required string CurrentL2 { get; init; }
    public required string CurrentL3 { get; init; }

    public required string VoltageL1 { get; init; }
    public required string VoltageL2 { get; init; }
    public required string VoltageL3 { get; init; }

    public required int TariffIndicator { get; init; }

    public required string Electricity1FromGrid { get; init; }
    public required string Electricity1ToGrid { get; init; }

    public required string Electricity2FromGrid { get; init; }
    public required string Electricity2ToGrid { get; init; }
}