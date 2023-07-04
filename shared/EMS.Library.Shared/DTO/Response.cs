using System.Text.Json.Serialization;
using EMS.Library.Shared.DTO.Health;
using EMS.Library.Shared.DTO.Users;

namespace EMS.Library.Shared.DTO;

[JsonDerivedType(typeof(Response), typeDiscriminator: "Response")]
[JsonDerivedType(typeof(LoginResponse), typeDiscriminator: "LoginResponse")]
[JsonDerivedType(typeof(HealthResponse), typeDiscriminator: "HealthResponse")]
[JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators =true, UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
public class Response
{
    public required int Status { get; set; }
    public required string StatusText { get; init; }
    public string? Message { get; init; }

    public Response() { }

    [SetsRequiredMembers]
    public Response(int status, string statusText)
    {
        Status = status;
        StatusText = statusText;
    }
}