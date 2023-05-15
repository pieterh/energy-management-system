using System.Data.SqlTypes;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Extensions.DependencyInjection;

using EMS.Library;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Adapter.Solar;
using EMS.Library.Configuration;
using EMS.Library.Exceptions;
using EMS.Library.Tasks;
using EMS.Library.Xml;
using Enphase.DTO;
using Enphase.DTO.Home;
using Enphase.DTO.Info;

namespace Enphase;

public class EnphaseService : BackgroundService, ISolar
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly Uri _baseUri;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly TimeSpan TimeOut = new TimeSpan(0, 0, 120);

    private static string httpClientName = "Envoy";

    private static readonly MediaTypeWithQualityHeaderValue requestHeaderAcceptJson = new MediaTypeWithQualityHeaderValue("application/json");
    private static readonly MediaTypeWithQualityHeaderValue requestHeaderAcceptXml = new MediaTypeWithQualityHeaderValue("application/xml");

    internal Task? _backgroundTask;

    public static void ConfigureServices(IServiceCollection services, AdapterInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        var config = instance.Config;
        var baseUri = new Uri(config.EndPoint);
        var clientBuilder = services.AddHttpClient(httpClientName);
        clientBuilder.ConfigurePrimaryHttpMessageHandler(
            handler => new HttpClientHandler
            {
                Credentials = new CredentialCache { {
                    baseUri,
                    "Digest",
                    new NetworkCredential(config.Username, config.Password) } }
            });
        BackgroundServiceHelper.CreateAndStart<ISolar, EnphaseService>(services, instance.Config);
    }

    public EnphaseService(InstanceConfiguration config, IHttpClientFactory httpClientFactory)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        _baseUri = new Uri(config.EndPoint);

        _httpClientFactory = httpClientFactory;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            DisposeBackgroundTask();
        }

        base.Dispose(disposing);
    }

    private void DisposeBackgroundTask()
    {
        if (_backgroundTask is not null)
        {
            // we need atleast a minimal wait for the background task to finish.
            // but we extend it when we are not beeing canceled
            TaskTools.Wait(_backgroundTask, 500);
            TaskTools.Wait(_backgroundTask, 4500, TokenSource.Token);
            _backgroundTask.Dispose();
            _backgroundTask = null;
        }
    }

    protected override Task Start()
    {
        _backgroundTask = Task.Run(async () =>
        {
            Logger.Trace("BackgroundTask running");

            try
            {
                var i = await GetSystemInfo().ConfigureAwait(false);
                Logger.Info("Serial number      {SerialNumber}", i.Device.SerialNumber);
                Logger.Info("Part number        {PartNumber}", i.Device.PartNumber);
                Logger.Info("Software           {Software}", i.Device.Software);
                Logger.Info("API Version        {ApiVersion}", i.Device.ApiVersion);
                Logger.Info("Build ID           {BuildId}", i.BuildInfo.BuildId);
                Logger.Info("Build At           {BuildAt}", i.BuildInfo.BuildDate.ToString("u"));

                bool run;
                do
                {
                    var s = await GetProductionStatus().ConfigureAwait(false);
                    Logger.Info("Production         {s}", s ? "Forced off" : "On");

                    var inverters = await GetInverters().ConfigureAwait(false);
                    var sum = inverters.Sum((x) => x.LastReportWatts);
                    Logger.Info("Current production {Watts}", sum);

                    run = !await StopRequested(120000).ConfigureAwait(false);
                }
                while (run);
            }
            catch (OperationCanceledException e)
            {
                /* We expecting the cancelation exception and don't need to act on it */
                if (!TokenSource.IsCancellationRequested)
                {
                    Logger.Error(e, "Unexpected error");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unhandled exception in BackgroundTask");
                throw;
            }

            if (TokenSource.Token.IsCancellationRequested)
                Logger.Info("Canceled");

            Logger.Trace("BackgroundTask stopped -> stop requested {StopRequested}", StopRequested(0));
        }, TokenSource.Token);

        return _backgroundTask;
    }

    protected override void Stop()
    {
        /* wait a bit for the background task in the case that it still is trying to connect */
        TaskTools.Wait(_backgroundTask, 15000);

        DisposeBackgroundTask();
    }

    #region ISolar
    public async Task<bool> StopProduction()
    {
        try
        {
            var p = new ProductionSwitchResponse() { Length = 1, Arr = new int[] { 1 } };

            var str = JsonSerializer.Serialize<ProductionSwitchResponse>(p);
            var buffer = System.Text.Encoding.UTF8.GetBytes(str);
            using var byteContent = new ByteArrayContent(buffer);
            var r = await PutData("/ivp/mod/603980032/mode/power", byteContent).ConfigureAwait(false);
            Logger.Info(r);
            return true;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("StopProduction => failed due task cancelation");
            return false;
        }
    }

    public async Task<bool> StartProduction()
    {
        try
        {
            var p = new ProductionSwitchResponse() { Length = 1, Arr = new int[] { 0 } };

            var str = JsonSerializer.Serialize<ProductionSwitchResponse>(p);
            var buffer = System.Text.Encoding.UTF8.GetBytes(str);
            using var byteContent = new ByteArrayContent(buffer);
            var r = await PutData("/ivp/mod/603980032/mode/power", byteContent).ConfigureAwait(false);
            Logger.Info(r);
            return true;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("StartProduction => failed due task cancelation");
            return false;
        }
    }

    /// <summary>
    /// Gets the production status
    /// </summary>
    /// <returns>true the production is forced off </returns>
    public async Task<bool> GetProductionStatus()
    {
        try
        {
            var productionStatusResponse = await GetJSonData<ProductionStatusResponse>("/ivp/mod/603980032/mode/power").ConfigureAwait(false);
            return productionStatusResponse?.PowerForcedOff ?? true;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetProductionStatus => failed due task cancelation");
            return false;
        }
    }
    #endregion

    internal async Task<HomeResponse?> GetHomeInfo()
    {
        try
        {
            var home = await GetJSonData<HomeResponse>("/home.json").ConfigureAwait(false);
            return home;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetProductionStatus => failed due task cancelation");
            return new HomeResponse();
        }
    }

    internal async Task<InfoResponse> GetSystemInfo()
    {
        try
        {
            var sysInfo = await GetXmlData<InfoResponse>("/info.xml").ConfigureAwait(false);
            return sysInfo ?? new InfoResponse();
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetProductionStatus => failed due task cancelation");
            return new InfoResponse();
        }
    }

    internal async Task<Inverter[]> GetInverters()
    {
        try
        {
            var inverters = await GetJSonData<Inverter[]>("/api/v1/production/inverters").ConfigureAwait(false);
            return inverters ?? Array.Empty<Inverter>();
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetProductionStatus => failed due task cancelation");
            return Array.Empty<Inverter>();
        }
    }

    #region Helpers
    internal async Task<T?> GetJSonData<T>(string endpoint)
    {
        var data = await GetData<T?>(endpoint, requestHeaderAcceptJson, async (response, cancellationToken) =>
        {
            if (response.IsSuccessStatusCode)
            {
                var inverters = await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken).ConfigureAwait(false);
                return inverters;
            }
            else
            {
                Logger.Error("Error retrieving inverter information status {StatusCode}, {Reason}", (int)response.StatusCode, response.ReasonPhrase);
                return default(T);
            }
        }).ConfigureAwait(false);

        return data;
    }

    internal async Task<T?> GetXmlData<T>(string endpoint)
    {
        var data = await GetData<T?>(endpoint, requestHeaderAcceptXml, async (response, cancellationToken) =>
        {
            if (response.IsSuccessStatusCode)
            {
                var xmlString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var obj = XmlHelpers.XmlToObject<T>(xmlString);
                return obj;
            }
            else
            {
                Logger.Error("Error retrieving inverter information status {StatusCode}, {Reason}", (int)response.StatusCode, response.ReasonPhrase);
                return default(T);
            }
        }).ConfigureAwait(false);

        return data;
    }

    /// <summary>
    /// Raw method to perform an async get and return the response object with help of the responseHandler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint"></param>
    /// <param name="responseHandler">responsible for retrieving the content and transforming to the result data object</param>
    /// <returns></returns>
    internal virtual async Task<T> GetData<T>(string endpoint, MediaTypeWithQualityHeaderValue acceptMediaTypes, Func<HttpResponseMessage, CancellationToken, Task<T>> responseHandler)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeOut);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, TokenSource.Token);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Accept.Add(acceptMediaTypes);
        var requestUri = new Uri(_baseUri, endpoint);

        using var response = await client.GetAsync(requestUri, linkedTokenSource.Token).ConfigureAwait(false);

        T retval = await responseHandler(response, linkedTokenSource.Token).ConfigureAwait(false);
        return retval;
    }

    internal virtual async Task<string> PutData(string endpoint, HttpContent content)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeOut);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, TokenSource.Token);

        using var client = CreateClient();
        var requestUri = new Uri(_baseUri, endpoint);

        using var response = await client.PutAsync(requestUri, content, linkedTokenSource.Token).ConfigureAwait(false);
        var r = await response.Content.ReadAsStringAsync(linkedTokenSource.Token).ConfigureAwait(false);
        return r;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient(httpClientName);
    }
    #endregion
}