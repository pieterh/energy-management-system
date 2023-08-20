using System.ComponentModel.DataAnnotations;

namespace EMS.Library.Shared.DTO.Users;

public class SetPasswordModel
{
    [Required]
    [MinLength(11)]
    public required string OldPassword { get; init; }
    [Required]
    [MinLength(11)]
    public required string NewPassword { get; init; }
}