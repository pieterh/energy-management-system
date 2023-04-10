using System;
using System.Text.Json.Serialization;
using EMS.BlazorWasm.Client.Services.Auth;

namespace EMS.BlazorWasm.Client.Services.Chargepoint
{
	public interface IChargepointService
	{
        Task<StationInfoResponse> GetStationInfoAsync(CancellationToken cancellationToken);
        Task<SocketInfoResponse> GetSessionInfoAsync(int socket, CancellationToken cancellationToken);
    }

    public record StationInfoResponse : Response
    {
        public ProductInfoModel ProductInfo { get; init; } = default!;
        public StationStatusInfoModel StationStatus { get; set; } = default!;
    }

    public record SocketInfoResponse : Response
    {
        public SocketR socketInfo { get; set; } = default!;
        public SessionR sessionInfo { get; set; } = default!;
    }

    public record ProductInfoModel
    {
        public string Name { get; init; } = default!;
        public string Manufacturer { get; init; } = default!;
        public string FirmwareVersion { get; set; } = default!;
        public string Model { get; set; } = default!;
        public string StationSerial { get; set; } = default!;
        public long Uptime { get; set; } 
    }

    public record StationStatusInfoModel
    {
        public float ActiveMaxCurrent { get; set; }
        public float Temperature { get; set; } 
        public string OCCPState { get; set; } = default!;
        public uint NrOfSockets { get; set; } 
    }

    public record SessionR
    {
        public string start { get; set; } = default!;
        public int chargingTime { get; set; } = default!;
        public string energyDeliveredFormatted { get; set; } = default!;
    }

    public record SocketR
    {
        public int id { get; set; } = default!;
        public string voltageFormatted { get; set; } = default!;
        public string currentFormatted { get; set; } = default!;
        public string realEnergyDeliveredFormatted { get; set; } = default!;
        public bool availability { get; set; } = default!;
        public string mode3State { get; set; } = default!;
        public string mode3StateMessage { get; set; } = default!;
        public string? lastChargingStateChanged { get; set; }
        public bool vehicleIsConnected { get; set; } = default!;
        public bool vehicleIsCharging { get; set; } = default!;
        public int phases { get; set; } = default!;
        public string appliedMaxCurrent { get; set; } = default!;
        public string maxCurrent { get; set; } = default!;
        public string powerAvailableFormatted { get; set; } = default!;
        public string powerUsingFormatted { get; set; } = default!;
    }
}

