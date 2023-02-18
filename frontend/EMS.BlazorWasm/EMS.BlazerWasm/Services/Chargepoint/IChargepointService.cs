using System;
using EMS.BlazorWasm.Client.Services.Auth;

namespace EMS.BlazorWasm.Client.Services.Chargepoint
{
	public interface IChargepointService
	{
        Task<SocketInfoResponse> GetSessionInfoAsync(int socket);
    }

    public record SocketInfoResponse : Response
    {
        public SocketR socketInfo { get; set; } = default!;
        public SessionR sessionInfo { get; set; } = default!;
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

