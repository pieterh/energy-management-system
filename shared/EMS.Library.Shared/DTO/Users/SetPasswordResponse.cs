namespace EMS.Library.Shared.DTO.Users;

public class SetPasswordResponse : Response
{
    public UserModelBasic? User { get; init; }
}