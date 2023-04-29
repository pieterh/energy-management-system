namespace EMS.Library.Shared.DTO.Users;

public class PingResponse : Response
{
    public required UserModelBasic User { get; init; }
}