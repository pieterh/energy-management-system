using System.Buffers;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Cron;
using EMS.Library.JSon;
using EMS.Library.TestableDateTime;
using EMS.Library.Exceptions;

namespace EPEXSPOT;

[SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
public class EPEXSPOTService : BackgroundWorker, IPriceProvider
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private static readonly Uri _schemaUri = new("https://mijn.easyenergy.com");
    private const string _schemaResourceName = "getapxtariffs.schema.json";

    private readonly Crontab _cron = new("05 * * * *");
    // interval is every hour and we allow 2 minutes of slack
    private const int _watchdogms = (62 * 60) * 1000;

    private readonly string _endpoint;              // ie. https://mijn.easyenergy.com
    private readonly IHttpClientFactory _httpClientFactory;
    private static string httpClientName = "EPEXSpot";

    internal const string getapxtariffsMethod = "/nl/api/tariff/getapxtariffs";
    private const Decimal INKOOP = 0.01331m;     // inkoopkosten per kWh (incl. btw)
    private const Decimal ODE = 0.03691m;        // opslag doorzame energie per kWh (incl. btw)
    private const Decimal EB = 0.04452m;         // energie belasting per kWh (incl. btw)

    private Tariff[] _tariffs = Array.Empty<Tariff>();   // time sorted array of tariffs that where fetched


    public static void ConfigureServices(IServiceCollection services, AdapterInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        BackgroundServiceHelper.CreateAndStart<IPriceProvider, EPEXSPOTService>(services, instance.Config);
    }

    static EPEXSPOTService()
    {
        JsonHelpers.LoadAndRegisterSchema(_schemaUri, _schemaResourceName);
    }

    public EPEXSPOTService(InstanceConfiguration config, IHttpClientFactory httpClientFactory, IWatchdog watchdog) : base(watchdog)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(watchdog);

        Logger.Info($"EPEXSPOT({config.ToString().Replace(Environment.NewLine, " ", StringComparison.Ordinal)})");

        _endpoint = !string.IsNullOrWhiteSpace(config.EndPoint) ? config.EndPoint : string.Empty;
        _httpClientFactory = httpClientFactory;
    }

    protected override DateTimeOffset GetNextOccurrence()
    {
        var now = DateTimeOffset.Now;
        var nextRun = _cron.GetNextOccurrence(now);
        return nextRun;
    }

    protected override int GetInterval()
    {
        return _watchdogms;
    }

    protected override async Task DoBackgroundWork()
    {
        try
        {
            await HandleWork().ConfigureAwait(false);
            WatchDogTick();
        }
        catch (CommunicationException ce)
        {
            Logger.Error($"CommunicationException {ce.Message}");
        }        
    }

    internal async Task HandleWork()
    {
        _tariffs = RemoveOld(_tariffs);

        // we only need to look 12 hours forward
        if (_tariffs.Length > 12) return;

        // grab more tariffs 
        var t = await GetTariff(DateTimeProvider.Now.Date, DateTimeProvider.Now.Date.AddDays(2)).ConfigureAwait(false);

        if (t != null && t.Length > 0)
            _tariffs = RemoveOld(t);
    }

    /// <summary>
    /// Get the current tariff from cache
    /// </summary>
    /// <returns></returns>
    public Tariff? GetTariff()
    {
        var tariff = FindTariff(_tariffs, DateTimeProvider.Now);
        return tariff;
    }

    public Tariff? GetNextTariff()
    {
        DateTime d = DateTimeProvider.Now.ToUniversalTime();
        var next = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0, DateTimeKind.Utc).AddHours(1);
        var tariff = FindTariff(_tariffs, next);
        return tariff;
    }

    /// <summary>
    /// Gets the tarriffs in range
    /// Currently directly from the source
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    internal async Task<Tariff[]> GetTariff(DateTime start, DateTime end)
    {
        try
        {
            var getapxtariffsUri = new Uri(new Uri(_endpoint), getapxtariffsMethod);
            using var client = _httpClientFactory.CreateClient(httpClientName);
            return await GetTariff(client, getapxtariffsUri, start, end).ConfigureAwait(false);
        }
        catch (HttpRequestException hre)
        {
            throw new CommunicationException($"Error while retrieving tariff {hre.Message}", hre);
        }
    }

    public async Task<DateTime> GetTariffLastTimestamp()
    {
        using var client = _httpClientFactory.CreateClient(httpClientName);

        var getapxtariffslasttimestampUri = new Uri(new Uri(_endpoint), "/nl/api/tariff/getapxtariffslasttimestamp");
        using var resultStream = await client.GetStreamAsync(getapxtariffslasttimestampUri).ConfigureAwait(false);
        using var streamReader = new StreamReader(resultStream);
        var result = JsonSerializer.Deserialize<DateTime>(streamReader.BaseStream, new JsonSerializerOptions());

        Logger.Info($"tariff lasttime -> {result}");
        return result;
    }

    internal static async Task<Tariff[]> GetTariff(HttpClient client, Uri getapxtariffsUri, DateTime start, DateTime end)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(getapxtariffsUri);

        var rawTariffData = await GetRawTariffData(client, getapxtariffsUri, start, end).ConfigureAwait(false);

        Tariff[] resultArray = GetTariffFromRaw(rawTariffData);

        return resultArray;
    }

    internal static async Task<byte[]> GetRawTariffData(HttpClient client, Uri getapxtariffsUri, DateTime start, DateTime end)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(getapxtariffsUri);

        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();

        var queryString = new Dictionary<string, string?>
        {
            { "startTimestamp", startUtc.ToString("o") },
            { "endTimestamp", endUtc.ToString("o") }
        };

        Uri uri = new(Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(getapxtariffsUri.ToString(), queryString));

        var httpResponseMessage = await client.GetAsync(uri).ConfigureAwait(false);

        byte[] resultArray;
        if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
        {
            resultArray = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }
        else
        {
            Logger.Error("There was an error reading from the tariff service. {StatusCode}", httpResponseMessage.StatusCode);
            resultArray = Array.Empty<byte>();
        }

        return resultArray;
    }

    internal static Tariff[] GetTariffFromRaw(byte[] rawTariffData)
    {
        Tariff[] resultArray;

        try
        {
            using var jsondocument = JsonDocument.Parse(rawTariffData);
            var jsonResult = jsondocument.RootElement;
            var isValid = JsonHelpers.Evaluate(_schemaResourceName, jsonResult);

            if (isValid)
            {
                var result = JsonSerializer.Deserialize<SpotTariff[]>(jsonResult, new JsonSerializerOptions());

                // calculate consumer price by adding the 'opslag duurzame energie', 'energie belasting' and 'inkoopkosten'
                var r = from x in result
                        select new Tariff(x.Timestamp, x.TariffUsage + ODE + EB + INKOOP, x.TariffReturn);

                // create array to return and make sure it is sorted
                resultArray = r.ToArray();
                Array.Sort(resultArray, (x, y) => x.Timestamp.CompareTo(y.Timestamp));
            }
            else
            {
                Logger.Error("There was an error in the format of the tariff service.");
                resultArray = Array.Empty<Tariff>();
            }
        }
        catch (JsonException je)
        {
            Logger.Error(je, "There was an error parsing the supplied data as json.");
            resultArray = Array.Empty<Tariff>();
        }

        return resultArray;
    }

    /// <summary>
    /// Find the correct tariff based on given time
    /// </summary>
    /// <param name="t">time sorted array of tariffs</param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static Tariff? FindTariff(Tariff[] t, DateTime start)
    {
        if (t == null || t.Length == 0) return null;
        if (start.Kind == DateTimeKind.Unspecified) throw new ArgumentOutOfRangeException(nameof(start));

        var startUtc = start.ToUniversalTime();
        var idx = t.ToList().FindIndex(x => { return x.Timestamp >= startUtc; });

        if (idx < 0) return null;
        if (t[idx].Timestamp.Equals(start)) return t[idx];
        if (idx == 0) return null;
        return t[idx - 1];
    }

    /// <summary>
    /// Removes old tariffs and returns a sorted array
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    internal static Tariff[] RemoveOld(IEnumerable<Tariff> t)
    {
        var t2 = t.Where((x) => x.Timestamp > DateTimeProvider.Now.ToUniversalTime().AddHours(-2)).ToArray();
        Array.Sort(t2, (x, y) => { return x.Timestamp.CompareTo(y.Timestamp); });
        return t2;
    }
}