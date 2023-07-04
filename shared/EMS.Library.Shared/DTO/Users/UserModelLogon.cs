namespace EMS.Library.Shared.DTO.Users;

public class UserModelLogon : UserModelBasic
{
    public required bool NeedPasswordChange { get; init; }

    [SetsRequiredMembers]
    public UserModelLogon(Guid id, string username, string name, bool needPasswordChange) : base(id, username, name)
    {
        NeedPasswordChange = needPasswordChange;
    }
}