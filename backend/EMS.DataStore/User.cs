namespace EMS.DataStore;

[Index(nameof(Username), IsUnique = true)]
public record User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ID { get; set; }

    private string _username = string.Empty;    
    public required string Username
    {
        get => _username;
        set
        {
            ArgumentNullException.ThrowIfNullOrEmpty(value);
            _username = value.ToUpperInvariant();
        }
    }

    public required string Name { get; set; }    
    public required string Password { get; set; }

    public DateTime LastPasswordChangedDate { get; set; }
    public DateTime LastLogonDate { get; set; }

    protected virtual bool PrintMembers(StringBuilder stringBuilder)
    {
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));

        stringBuilder.Append($"ID = {ID}, ");
        stringBuilder.Append($"Username = {Username}, ");
        stringBuilder.Append($"Name = {Name}, ");
        stringBuilder.Append($"LastPasswordChangedDate = {LastPasswordChangedDate.ToLocalTime():0}, ");
        stringBuilder.Append($"LastLogon = {LastLogonDate.ToLocalTime():O}, ");
        return true;
    }
}