namespace EMS.Library.Shared.DTO;

public class Response
{
    public required int Status { get; set; }
    public required string StatusText { get; init; }
    public string? Message { get; init; }
}