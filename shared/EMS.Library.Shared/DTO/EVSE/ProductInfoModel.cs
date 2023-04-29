namespace EMS.Library.Shared.DTO.EVSE;

public record ProductInfoModel
{
    public required string Name { get; init; }
    public required string Manufacturer { get; init; }
    public required string FirmwareVersion { get; init; }
    public required string Model { get; init; }
    public required string StationSerial { get; init; }
    public required long Uptime { get; init; }
    public required DateTime DateTimeUtc { get; init; }
}