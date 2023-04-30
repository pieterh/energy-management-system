using EMS.Library.Shared.DTO.EVSE;

namespace EMS.BlazorWasm.Client.Services.Chargepoint;

public interface IChargepointService
{
    Task<StationInfoResponse> GetStationInfoAsync(CancellationToken cancellationToken);
    Task<SocketInfoResponse> GetSessionInfoAsync(int socket, CancellationToken cancellationToken);
}