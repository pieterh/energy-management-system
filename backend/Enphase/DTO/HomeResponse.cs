
using System.Text.Json.Serialization;

namespace Enphase.DTO.Home;

internal record class HomeResponse
{
    [JsonPropertyName("software_build_epoch")]
    public int SoftwareBuildEpoch { get; set; }

    [JsonPropertyName("network")]
    public NetworkResponse Network { get; set; } = default!;
}

internal record class NetworkResponse
{
    [JsonPropertyName("primary_interface")]
    public string PrimaryInterface { get; set; } = default!;
    [JsonPropertyName("interfaces")]
    public InterfaceResponse[] Interfaces { get; set; } = default!;
}

internal record class InterfaceResponse
{
    [JsonPropertyName("interface")]
    public string Interface { get; set; } = default!;

    [JsonPropertyName("mac")]
    public string Mac { get; set; } = default!;

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = default!;

    [JsonPropertyName("dhcp")]
    public bool Dhcp { get; set; }

    [JsonPropertyName("carrier")]
    public bool Carrier { get; set; }

    [JsonPropertyName("signal_strength")]
    public int SignalStrength { get; set; }

    [JsonPropertyName("signal_strength_max")]
    public int SignalStrengthMax { get; set; }
}