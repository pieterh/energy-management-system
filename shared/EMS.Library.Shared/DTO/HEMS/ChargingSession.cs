namespace EMS.Library.Shared.DTO.HEMS;

public class ChargingSession
{
    public required DateTimeOffset Timestamp { get; init; }
    public required decimal EnergyDelivered { get; init; }
    public required decimal Cost { get; init; }
    public required decimal Price { get; init; }
}