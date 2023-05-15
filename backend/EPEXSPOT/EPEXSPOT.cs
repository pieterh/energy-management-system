using System.Buffers;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using EMS.Library.Adapter.PriceProvider;
using EMS.Library.JSon;
using EMS.Library.TestableDateTime;
using System.Text;

namespace EPEXSPOT
{
    [SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
    public class EPEXSPOTService : Microsoft.Extensions.Hosting.BackgroundService, IPriceProvider
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed;

        private static readonly Uri _schemaUri = new("https://mijn.easyenergy.com");
        private const string _schemaResourceName = "getapxtariffs.schema.json";

        private readonly string _endpoint;              // ie. https://mijn.easyenergy.com
        private readonly IHttpClientFactory _httpClientFactory;

        internal const string getapxtariffsMethod = "/nl/api/tariff/getapxtariffs";
        private const Decimal INKOOP = 0.01331m;     // inkoopkosten per kWh (incl. btw)
        private const Decimal ODE = 0.03691m;        // opslag doorzame energie per kWh (incl. btw)
        private const Decimal EB = 0.04452m;         // energie belasting per kWh (incl. btw)

        private Tariff[] _tariffs = Array.Empty<Tariff>();   // time sorted array of tariffs that where fetched

        public bool isDisposed { get => _disposed; }

        public static void ConfigureServices(IServiceCollection services, AdapterInstance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);
            BackgroundServiceHelper.CreateAndStart<IPriceProvider, EPEXSPOTService>(services, instance.Config);
        }

        static EPEXSPOTService()
        {
            JsonHelpers.LoadAndRegisterSchema(_schemaUri, _schemaResourceName);
        }

        public EPEXSPOTService(InstanceConfiguration config, IHttpClientFactory httpClientFactory)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            Logger.Info($"EPEXSPOT({config.ToString().Replace(Environment.NewLine, " ", StringComparison.Ordinal)})");

            _endpoint = !string.IsNullOrWhiteSpace(config.EndPoint) ? config.EndPoint : string.Empty;
            _httpClientFactory = httpClientFactory;
        }

        public override void Dispose()
        {
            Grind(true);
            base.Dispose();
            GC.SuppressFinalize(this);  // Suppress finalization.
        }

        // calling this method grind to keep sonar happy
        // unfortunately the base class doesn't implement the dispose pattern
        // properly, so there is no method Dispose(bool disposing) to override...
        protected virtual void Grind(bool disposing)
        {
            Logger.Trace($"Dispose({disposing}) _disposed {_disposed}");

            if (_disposed) return;

            _disposed = true;
            Logger.Trace($"Dispose({disposing}) done => _disposed {_disposed}");
        }

        [SuppressMessage("Code Analysis", "CA1031")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.Info($"EPEXSPOT Starting");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        HandleWork();

                        // pause for five minutes, before we handle some work again
                        await Task.Delay(60000 * 5, stoppingToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException tce)
                    {
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            Logger.Error("Exception: " + tce.Message);
                        }
                    }
                    catch (Exception e) when (e.Message.StartsWith("Partial exception packet", StringComparison.Ordinal))
                    {
                        Logger.Error("Partial Modbus packaged received, we try later again");
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Exception: " + e.Message);
                        Logger.Error("Unhandled, we try later again");
                        await Delay(2500, stoppingToken).ConfigureAwait(false);
                    }
                }
                Logger.Info($"Canceled");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unhandled exception");
            }
        }

        private static async Task Delay(int millisecondsDelay, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(millisecondsDelay, stoppingToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { /* ignoring */ }
        }

        internal void HandleWork()
        {
            _tariffs = RemoveOld(_tariffs);

            // we only need to look 12 hours forward
            if (_tariffs.Length > 12) return;

            // grab more tariffs 
            var t = GetTariff(DateTimeProvider.Now.Date, DateTimeProvider.Now.Date.AddDays(2)).Result;

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
            var getapxtariffsUri = new Uri(new Uri(_endpoint), getapxtariffsMethod);
            using var client = _httpClientFactory.CreateClient();
            return await GetTariff(client, getapxtariffsUri, start, end).ConfigureAwait(false);
        }

        public async Task<DateTime> GetTariffLastTimestamp()
        {
            using var client = _httpClientFactory.CreateClient();

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
                using var stream = new MemoryStream();
                using Utf8JsonWriter w = new Utf8JsonWriter(stream);
                jsondocument.WriteTo(w);
                w.Flush();
                var str = Encoding.UTF8.GetString(stream.ToArray());
                Console.WriteLine(str);
                Logger.Info(str);
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
}
