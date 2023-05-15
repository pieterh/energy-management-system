using System;
using System.Text.Json.Serialization;

namespace Enphase.DTO;

internal record ProductionStatusResponse
{
    [JsonPropertyName("powerForcedOff")]
    public bool PowerForcedOff { get; set; }
}

internal record ProductionSwitchResponse
{
    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("arr")]
    public int[] Arr { get; set; } = default!;
}