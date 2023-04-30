namespace EMS.Library.Shared.DTO.Users;

public class UserModelBasic
{
    public Guid Id { get; set; }
    public required string Username { get; init; }
    public required string Name { get; init; }

    public UserModelBasic() { }

    [SetsRequiredMembers]
    public UserModelBasic(Guid id, string username, string name) {
        Id = id;
        Username = username;
        Name = name;
    }
}