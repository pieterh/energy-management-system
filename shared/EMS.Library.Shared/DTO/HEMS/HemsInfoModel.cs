namespace EMS.Library.Shared.DTO.HEMS;

public class HemsInfoModel
{
    public required string Mode { get; init; }
    public required string State { get; init; }
    public required DateTime LastStateChange { get; init; }
    public required string CurrentAvailableL1Formatted { get; init; }
    public required string CurrentAvailableL2Formatted { get; init; }
    public required string CurrentAvailableL3Formatted { get; init; }
}