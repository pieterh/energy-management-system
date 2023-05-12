using System.Text.Json.Serialization;

namespace EMS.Library.Shared.DTO.Health;

[JsonDerivedType(typeof(HealthResponse), typeDiscriminator: "HealthResponse")]
public class HealthResponse : Response
{
    public required DateTime UpSince { get; init; }

    [SetsRequiredMembers]
    public HealthResponse(DateTime upSince)  {
        Status = 200;
        StatusText = "OK";
        UpSince = upSince;
    }
}