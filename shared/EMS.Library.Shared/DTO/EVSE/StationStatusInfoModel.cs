namespace EMS.Library.Shared.DTO.EVSE;

public record StationStatusInfoModel
{
    public required float ActiveMaxCurrent { get; init; }
    public required float Temperature { get; init; }
    public required string OCCPState { get; init; }
    public required uint NrOfSockets { get; init; }
}