namespace EMS.Library.Shared.DTO.Users;

public class SetPasswordModel
{
    public required string OldPassword { get; init; }
    public required string NewPassword { get; init; }
}