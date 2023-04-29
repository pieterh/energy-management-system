namespace EMS.Library.Shared.DTO.SmartMeter;

public class SmartMeterInfoResponse : Response
{
    public required SmartMeterInfoModel Info { get; init; }
}