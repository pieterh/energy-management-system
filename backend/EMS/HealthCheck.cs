using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

using NLog;

using EMS.Library;
using EMS.Library.Exceptions;
using EMS.Library.JSon;
using EMS.Library.Shared.DTO;
using EMS.Library.Shared.DTO.Health;
using EMS.WebHost;

namespace EMS;

internal class HealthCheck
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly MediaTypeWithQualityHeaderValue requestHeaderAccept = new MediaTypeWithQualityHeaderValue("application/json");

    private Uri RequestUri { get; init; }

    private static TimeSpan Timeout = new TimeSpan(0, 0, 15);

    public HealthCheck(int port)
    {
        RequestUri = new UriBuilder("http", "localhost", port, "api/health/check").Uri;
    }

    /// <summary>
    /// Perform a health check on the local system for the given port.
    /// </summary>
    /// <returns>0 = all okay, 1 = there is a problem</returns>
    public async Task<int> PerformHealthCheck()
    {
        int status = 1;
        try
        {
            var c = await RetrieveHealthStatus().ConfigureAwait(false);

            if (c?.Status == 200)
            {
                var hc = (HealthResponse)c;
                status = 0;
                Console.WriteLine($"UpSince {hc.UpSince}");
                Console.WriteLine("Health is ok.");
            }
        }
        catch (NullException ne)
        {
            Logger.Error(ne, "There was a problem retrieving health status.");
        }
        if (status != 0)
            Logger.Info("Health is NOT ok");
        return status;
    }

    /// <summary>
    /// Retrieves the helath status from the running system
    /// </summary>
    /// <returns></returns>
    internal virtual async Task<Response?> RetrieveHealthStatus()
    {
        Response? c = default;

        try
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(requestHeaderAccept);
            using var response = await client.GetAsync(RequestUri, cancellationTokenSource.Token).ConfigureAwait(false);
            NullException.ThrowIfNull(response, "No response from health check");

            c = await response.Content.ReadFromJsonAsync<Response>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationTokenSource.Token).ConfigureAwait(false);
            NullException.ThrowIfNull(c, "No data in response from health check");
        }
        catch (HttpRequestException hre)
        {
            Logger.Error(hre, "Exception while retrieving health status.");
        }
        catch (TaskCanceledException tce)
        {
            Logger.Error(tce, "Timeout while retrieving health status.");
        }
        return c;
    }
}