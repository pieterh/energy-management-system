using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using EMS.BlazorWasm.Shared.Exceptions;

namespace EMS.BlazorWasm.Client.Services.Chargepoint
{
	public class ChargepointService : IChargepointService
	{
        private readonly HttpClient _httpClient;
        public ChargepointService(HttpClient httpClient)
		{
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            _httpClient = httpClient;
        }

        public async Task<SocketInfoResponse> GetSessionInfoAsync(int socket)
        {
            return await GetSessionInfoAsync(socket, CancellationToken.None);
        }

        public async Task<SocketInfoResponse> GetSessionInfoAsync(int socket, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"api/evse/socket/{socket}");
            switch (response.StatusCode) {
                case System.Net.HttpStatusCode.OK:
                {
                    var socketInfoResponse = await response.Content.ReadFromJsonAsync<SocketInfoResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
                    if (socketInfoResponse == null)
                        throw new HEMSApplicationException("No response!");
                    return socketInfoResponse;
                }
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new HEMSApplicationException("Unauthorized!");
                default:
                    throw new HEMSApplicationException($"Uhh {response.StatusCode}");
            }
        }
    }
}

