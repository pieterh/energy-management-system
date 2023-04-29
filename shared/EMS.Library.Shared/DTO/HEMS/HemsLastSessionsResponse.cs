namespace EMS.Library.Shared.DTO.HEMS;

public class HemsLastSessionsResponse : Response
{
    public required IEnumerable<ChargingSession> Sessions { get; init; }
}