namespace EMS.Library.Shared.DTO.Users;

public class LoginResponse : Response
{
    public required string Token { get; init; }
    public required UserModelLogon User { get; init; }
    public void Deconstruct(out UserModelLogon? user, out string token)
    {
        user = User;
        token = Token;
    }
}