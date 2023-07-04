namespace EMS.Library.Shared.DTO.HEMS;

public class HemsInfoResponse : Response
{
    public required HemsInfoModel Info { get; init; }
    public required IEnumerable<Measurement> Measurements { get; init; }
}