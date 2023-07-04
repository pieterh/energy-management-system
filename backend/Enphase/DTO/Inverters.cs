using System.Text.Json.Serialization;
using Enphase.DTO.Home;

namespace Enphase.DTO;

internal record Inverter
{
    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; set; } = default!;

    [JsonPropertyName("lastReportDate")]
    public int LastResportDate { get; set; }

    [JsonPropertyName("devType")]
    public int DeviceType { get; set; }

    [JsonPropertyName("lastReportWatts")]
    public int LastReportWatts { get; set; }

    [JsonPropertyName("maxReportWatts")]
    public int MaxReportWatts { get; set; }
}