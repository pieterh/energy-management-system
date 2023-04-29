namespace EMS.Library.Shared.DTO.EVSE;

public class SessionInfoModel
{
    public required DateTime Start { get; init; }
    public required UInt32 ChargingTime { get; init; }
    public required string EnergyDeliveredFormatted { get; init; }                // kWh
}
