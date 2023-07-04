using System.Text.Json.Serialization;

namespace EMS.Library.Shared.DTO.EVSE;

public class SocketInfoModel
{
    public int Id { get; set; }

    public required string VoltageFormatted { get; init; }
    public required string CurrentFormatted { get; init; }

    public required string RealPowerSumFormatted { get; init; }                 // kW
    public required string RealEnergyDeliveredFormatted { get; init; }          // kWh


    public required bool Availability { get; init; }
    public required string Mode3State { get; init; }
    public required string Mode3StateMessage { get; init; }
    public required DateTime LastChargingStateChanged { get; init; }
    public required bool VehicleIsConnected { get; init; }
    public required bool VehicleIsCharging { get; init; }

    [JsonConverter(typeof(FloatConverterP1))]
    public required float AppliedMaxCurrent { get; init; }
    public required UInt32 MaxCurrentValidTime { get; init; }
    [JsonConverter(typeof(FloatConverterP1))]
    public required float MaxCurrent { get; init; }

    [JsonConverter(typeof(FloatConverterP0))]
    public required float ActiveLBSafeCurrent { get; init; }
    public required bool SetPointAccountedFor { get; init; }
    public required Phases Phases { get; init; }

    public required string PowerAvailableFormatted { get; init; }                 // kW
    public required string PowerUsingFormatted { get; init; }                     // kW
}
