namespace EMS.Library.Configuration;

public record class AdapterInstance
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool Enabled { get; set; } = true;
    public Guid AdapterId { get; set; }
    public InstanceConfiguration Config { get; set; } = default!;
}

public record InstanceConfiguration
{
    public string Type { get; set; } = default!;
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string Device { get; set; } = default!;
    public string EndPoint { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}