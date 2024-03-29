﻿using System.Collections;
using System.Data.SqlTypes;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
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
using EMS.Library.JSon;
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

    private static readonly TimeSpan TimeOut = new TimeSpan(0, 0, 0, 120, 0);

    private static string httpClientName = "Envoy";

    private static readonly MediaTypeWithQualityHeaderValue requestHeaderAcceptJson = new MediaTypeWithQualityHeaderValue("application/json");
    private static readonly MediaTypeWithQualityHeaderValue requestHeaderAcceptXml = new MediaTypeWithQualityHeaderValue("application/xml");
    private static readonly MediaTypeWithQualityHeaderValue requestHeaderAcceptTextHtml = new MediaTypeWithQualityHeaderValue("text/html");

    internal Task? _backgroundTask;

    public static void ConfigureServices(IServiceCollection services, AdapterInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var clientBuilder = services.AddHttpClient(httpClientName);
        
        // We need to set the network credentials early in the configuration fase
        // There is no quick way to retreive the serial number from the device and
        // calculate the password. Since I don't like to block startup untill the
        // serial is retrieved from the system, we just take it from the config file.
        // Atleast the startup is quick ;-)
        var username = instance.Config.Username;
        var serial = instance.Config.Password;
        var password = Authentication.GetMobilePasswdForSerial(serial, username);

        clientBuilder.ConfigurePrimaryHttpMessageHandler(
            handler => new HttpClientHandler
            {
                Credentials = new CredentialCache
                                    {
                                        {
                                            new Uri(instance.Config.EndPoint),
                                            "Digest",
                                            new NetworkCredential(username, password)
                                        }
                                    }
            });
        BackgroundServiceHelper.CreateAndStart<ISolar, EnphaseService>(services, instance.Config);
    }

    public EnphaseService(InstanceConfiguration config, IHttpClientFactory httpClientFactory, IWatchdog watchdog) : base(watchdog)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(watchdog);

        _baseUri = new Uri(config.EndPoint);
        _httpClientFactory = httpClientFactory;
    }

    private const int _intervalms = 120 * 1000;
    private const int _watchdogms = _intervalms + (150 * 1000);
    protected override DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(_intervalms);
    }

    protected override int GetInterval()
    {
        return _watchdogms;    // interval and expected max duration of execution
    }

    protected override async Task DoBackgroundWork()
    {
        try
        {
            var s = await GetProductionStatus(CancellationToken).ConfigureAwait(false);
            Logger.Info("Production         {s}", s ? "Forced off" : "On");

            var inverters = await GetInverters(CancellationToken).ConfigureAwait(false);
            var sum = inverters.Sum((x) => x.LastReportWatts);
            Logger.Debug("Current production {Watts}", sum);
            WatchDogTick();
        }
        catch (CommunicationException ce)
        {
            Logger.Error("CommunicationException {Message}", ce.Message);
        }
        catch (TaskCanceledException tce)
        {
            if (CancellationToken.IsCancellationRequested)
                Logger.Info("Task cancelled");
            else
                Logger.Warn(tce, "Unexpected task cancellation");
        }
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

        await base.Start().ConfigureAwait(false);
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

    /// <summary>
    /// Returns some system info (BackboneConfig?) found on the homepage as a dictionary.
    /// Not sure if we can use this or not
    ///     "serial":"122011110000"
    ///     "profiles":"false"
    ///     "show_prompt":"false"
    ///     "internal_meter":"true"
    ///     "software_version":"R4.10.35 (6ed292)"
    ///     "envoy_type":"EU"
    ///     "polling_interval":"300000"
    ///     "polling_frequency":"60"
    ///     "backbone_public":"true"
    ///     "cte_mode":"false"
    ///     "toolkit":"false"
    ///     "max_errors":"0"
    ///     "max_timeouts":"0"
    /// "e_units":"sig_fig"
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal async Task<IDictionary<string, string>> GetSystemInfoDictionary(CancellationToken cancellationToken)
    {
        var str = await GetData("/home", CancellationToken.None).ConfigureAwait(false);

        // grab the BackboneConfig configuration from the html
        // since it is javascript, the properties are not enclosed with quotes, so we can't just deserialize this.
        var rege = new Regex("BackboneConfig = ({[^{}]*})", RegexOptions.Multiline | RegexOptions.Singleline, new TimeSpan(0, 0, 0, 0, 200));
        var matches = rege.Match(str);

        Dictionary<string, string> retval;
        if (matches.Success && matches.Groups.Count == 2)
        {
            string javascript = matches.Groups[1].Value;
            retval = JsonHelpers.JavascriptObjectToDictionary(javascript);
        }
        else
            retval = new Dictionary<string, string>(0);
        return retval;
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
                Logger.Error("Error retrieving json data from enphase. Status {StatusCode}, {Reason}", (int)response.StatusCode, response.ReasonPhrase);
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
                Logger.Error("Error retrieving xml data from enphase. Status {StatusCode}, {Reason}", (int)response.StatusCode, response.ReasonPhrase);
                return default(T);
            }
        }, cancellationToken).ConfigureAwait(false);

        return data;
    }

    internal async Task<string> GetData(string endpoint, CancellationToken cancellationToken)
    {
        var data = await GetData<string>(endpoint, requestHeaderAcceptTextHtml, async (response, cancellationToken) =>
        {
            if (response.IsSuccessStatusCode)
            {
                var rawString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return rawString ?? string.Empty;
            }
            else
            {
                Logger.Error("Error retrieving xml data from enphase. Status {StatusCode}, {Reason}", (int)response.StatusCode, response.ReasonPhrase);
                return string.Empty;
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