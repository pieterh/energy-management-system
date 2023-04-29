namespace EMS.Library.Shared.DTO.EVSE;

public class StationInfoResponse : Response
{
    public required ProductInfoModel ProductInfo { get; init; }
    public required StationStatusInfoModel StationStatus { get; init; }
}