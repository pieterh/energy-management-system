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
using EMS.Library.TestableDateTime;
using EMS.Library.Xml;
using Enphase.DTO;
using Enphase.DTO.Home;
using Enphase.DTO.Info;

namespace Enphase;

public class EnphaseService : BackgroundWorker, ISolar
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly Uri _baseUri;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWatchdog _watchdog;

    private static readonly TimeSpan TimeOut = new TimeSpan(0, 0, 0, 120, 0);

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

    public EnphaseService(InstanceConfiguration config, IHttpClientFactory httpClientFactory, IWatchdog watchdog)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(watchdog);

        _baseUri = new Uri(config.EndPoint);

        _httpClientFactory = httpClientFactory;
        _watchdog = watchdog;
    }

    protected override DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(120000);
    }

    protected override async Task DoBackgroundWork()
    {
        var s = await GetProductionStatus(CancellationToken).ConfigureAwait(false);
        Logger.Info("Production         {s}", s ? "Forced off" : "On");

        var inverters = await GetInverters(CancellationToken).ConfigureAwait(false);
        var sum = inverters.Sum((x) => x.LastReportWatts);
        Logger.Info("Current production {Watts}", sum);

        _watchdog.Tick(this);
    }

    protected override async Task Start()
    {
        var i = await GetSystemInfo(CancellationToken).ConfigureAwait(false);
        Logger.Info("Serial number      {SerialNumber}", i.Device.SerialNumber);
        Logger.Info("Part number        {PartNumber}", i.Device.PartNumber);
        Logger.Info("Software           {Software}", i.Device.Software);
        Logger.Info("API Version        {ApiVersion}", i.Device.ApiVersion);
        Logger.Info("Build ID           {BuildId}", i.BuildInfo.BuildId);
        Logger.Info("Build At           {BuildAt}", i.BuildInfo.BuildDate.ToString("u"));

        _watchdog.Register(this, 120);
        await base.Start().ConfigureAwait(false);
    }

    protected override void Stop()
    {        
        base.Stop();
        _watchdog.Unregister(this);
    }

    #region ISolar
    public async Task<bool> StopProduction(CancellationToken cancellationToken)
    {
        try
        {
            var p = new ProductionSwitchResponse() { Length = 1, Arr = new int[] { 1 } };

            var str = JsonSerializer.Serialize<ProductionSwitchResponse>(p);
            var buffer = System.Text.Encoding.UTF8.GetBytes(str);
            using var byteContent = new ByteArrayContent(buffer);
            var r = await PutData("/ivp/mod/603980032/mode/power", byteContent, cancellationToken).ConfigureAwait(false);
            Logger.Info(r);
            return true;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("StopProduction => failed due task cancelation");
            return false;
        }
    }

    public async Task<bool> StartProduction(CancellationToken cancellationToken)
    {
        try
        {
            var p = new ProductionSwitchResponse() { Length = 1, Arr = new int[] { 0 } };

            var str = JsonSerializer.Serialize<ProductionSwitchResponse>(p);
            var buffer = System.Text.Encoding.UTF8.GetBytes(str);
            using var byteContent = new ByteArrayContent(buffer);
            var r = await PutData("/ivp/mod/603980032/mode/power", byteContent, cancellationToken).ConfigureAwait(false);
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
    public async Task<bool> GetProductionStatus(CancellationToken cancellationToken)
    {
        try
        {
            var productionStatusResponse = await GetJSonData<ProductionStatusResponse>("/ivp/mod/603980032/mode/power", cancellationToken).ConfigureAwait(false);
            return productionStatusResponse?.PowerForcedOff ?? true;
        }
        catch (CommunicationException ce)
        {
            Logger.Warn("GetProductionStatus => unable to retrieve status due to communication exception {message}", ce.Message);
            throw;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetProductionStatus => failed due task cancelation");
            throw;
        }
    }
    #endregion

    internal async Task<HomeResponse?> GetHomeInfo(CancellationToken cancellationToken)
    {
        try
        {
            var home = await GetJSonData<HomeResponse>("/home.json", cancellationToken).ConfigureAwait(false);
            return home;
        }
        catch (CommunicationException ce)
        {
            Logger.Warn("GetHomeInfo => unable to retrieve info due to communication exception {message}", ce.Message);
            throw;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetHomeInfo => failed due task cancelation");
            throw;
        }
    }

    internal async Task<InfoResponse> GetSystemInfo(CancellationToken cancellationToken)
    {
        try
        {
            var sysInfo = await GetXmlData<InfoResponse>("/info.xml", cancellationToken).ConfigureAwait(false);
            return sysInfo ?? new InfoResponse();
        }
        catch (CommunicationException ce)
        {
            Logger.Warn("GetSystemInfo => unable to retrieve info due to communication exception {message}", ce.Message);
            throw;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetSystemInfo => failed due task cancelation");
            throw;
        }
    }

    internal async Task<Inverter[]> GetInverters(CancellationToken cancellationToken)
    {
        try
        {
            var inverters = await GetJSonData<Inverter[]>("/api/v1/production/inverters", cancellationToken).ConfigureAwait(false);
            return inverters ?? Array.Empty<Inverter>();
        }
        catch (CommunicationException ce)
        {
            Logger.Warn("GetInverters => unable to retrieve inverters due to communication exception {message}", ce.Message);
            throw;
        }
        catch (TaskCanceledException)
        {
            Logger.Warn("GetInverters => failed due task cancelation");
            throw;
        }
    }

    #region Helpers
    internal async Task<T?> GetJSonData<T>(string endpoint, CancellationToken cancellationToken)
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
        }, cancellationToken).ConfigureAwait(false);

        return data;
    }

    internal async Task<T?> GetXmlData<T>(string endpoint, CancellationToken cancellationToken)
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
        }, cancellationToken).ConfigureAwait(false);

        return data;
    }

    /// <summary>
    /// Raw method to perform an async get and return the response object with help of the responseHandler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint"></param>
    /// <param name="responseHandler">responsible for retrieving the content and transforming to the result data object</param>
    /// <returns></returns>
    internal virtual async Task<T> GetData<T>(string endpoint, MediaTypeWithQualityHeaderValue acceptMediaTypes, Func<HttpResponseMessage, CancellationToken, Task<T>> responseHandler, CancellationToken cancellationToken)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeOut);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
        try
        {
            using var client = CreateClient();
            client.DefaultRequestHeaders.Accept.Add(acceptMediaTypes);
            var requestUri = new Uri(_baseUri, endpoint);

            using var response = await client.GetAsync(requestUri, linkedTokenSource.Token).ConfigureAwait(false);

            T retval = await responseHandler(response, linkedTokenSource.Token).ConfigureAwait(false);
            return retval;
        }
        catch (HttpRequestException hre)
        {
            throw new CommunicationException($"HttpRequestException while getting data from enphase. {hre.Message}", hre);
        }
        catch (OperationCanceledException oce)
        {
            if (cancellationTokenSource.IsCancellationRequested)
                throw new CommunicationException("Timeout while getting data from enphase.", oce);
            else
                throw; // parent task cancelled... bubble up
        }

    }

    internal virtual async Task<string> PutData(string endpoint, HttpContent content, CancellationToken cancellationToken)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeOut);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);

        try
        {
            using var client = CreateClient();
            var requestUri = new Uri(_baseUri, endpoint);

            using var response = await client.PutAsync(requestUri, content, linkedTokenSource.Token).ConfigureAwait(false);
            var r = await response.Content.ReadAsStringAsync(linkedTokenSource.Token).ConfigureAwait(false);
            return r;
        }
        catch (HttpRequestException hre)
        {
            throw new CommunicationException($"HttpRequestException while sending data to enphase. {hre.Message}", hre);
        }
        catch (OperationCanceledException oce)
        {
            if (cancellationTokenSource.IsCancellationRequested)
                throw new CommunicationException("Timeout while sending data to enphase.", oce);
            else
                throw; // parent task cancelled... bubble up
        }
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(httpClientName);

        // set the httpclient timeout just a bit longer then the timeout we use with the cancellation token.
        // this will make sure there is consistent behavior when there is a timeout
        client.Timeout = TimeOut.Duration().Add(new TimeSpan(0, 0, 2));
        return client;
    }
    #endregion
}