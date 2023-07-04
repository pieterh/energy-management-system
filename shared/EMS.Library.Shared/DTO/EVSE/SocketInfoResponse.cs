namespace EMS.Library.Shared.DTO.EVSE;

public class SocketInfoResponse : Response
{
    public required SocketInfoModel? SocketInfo { get; init; }
    public required SessionInfoModel? SessionInfo { get; init; }
}