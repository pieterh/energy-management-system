using System.Text.Json.Serialization;
using EMS.Library.Shared.DTO.Users;

namespace EMS.Library.Shared.DTO;

[JsonDerivedType(typeof(Response), typeDiscriminator: "Response")]
[JsonDerivedType(typeof(LoginResponse), typeDiscriminator: "LoginResponse")]
[JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators =true, UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
public class Response
{
    public required int Status { get; set; }
    public required string StatusText { get; init; }
    public string? Message { get; init; }
}