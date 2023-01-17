using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Json.Schema;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.JSon;
using EMS.Library.TestableDateTime;

namespace EPEXSPOT
{
    public class EPEXSPOT : Microsoft.Extensions.Hosting.BackgroundService, IPriceProvider
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed = false;

        private readonly string _endpoint;           // ie. https://mijn.easyenergy.com
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly Decimal INKOOP = 0.01331m;    // inkoopkosten per kWh (incl. btw)
        private readonly Decimal ODE = 0.03691m;       // opslag doorzame energie per kWh (incl. btw)
        private readonly Decimal EB = 0.04452m;        // energie belasting per kWh (incl. btw)

        private Tariff[] _tariffs = Array.Empty<Tariff>();   // time sorted array of tariffs that where fetched

        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services, Instance instance)
        {
            BackgroundServiceHelper.CreateAndStart<IPriceProvider, EPEXSPOT>(services, instance.Config);
        }

        public EPEXSPOT(Config config, IHttpClientFactory httpClientFactory)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            Logger.Info($"EPEXSPOT({config.ToString().Replace(Environment.NewLine, " ")})");
            
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info($"EPEXSPOT Starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    HandleWork();

                    // pause for five minutes, before we handle some work again
                    await Task.Delay(60000 * 5, stoppingToken);                    
                }
                catch (TaskCanceledException tce)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        Logger.Error("Exception: " + tce.Message);
                    }
                }
                catch (Exception e) when (e.Message.StartsWith("Partial exception packet"))
                {
                    Logger.Error("Partial Modbus packaged received, we try later again");
                }
                catch (Exception e)
                {
                    Logger.Error("Exception: " + e.Message);
                    Logger.Error("Unhandled, we try later again");
                    Logger.Error("Disposing connection");
                    await Task.Delay(2500, stoppingToken);
                }
            }
            Logger.Info($"Canceled");
        }

        private void HandleWork()
        {
            _tariffs = RemoveOld(_tariffs);

            if (_tariffs.Length > 12) return;

            var t = GetTariff(DateTimeProvider.Now.Date, DateTimeProvider.Now.Date.AddDays(2)).Result;
 
            _tariffs = RemoveOld(t);            
        }

        public Tariff? GetTariff()
        {
            var tariff = FindTariff(_tariffs, DateTimeProvider.Now);
            return tariff;
        }

        public async Task<Tariff[]> GetTariff(DateTime start, DateTime end)
        {
            var startUtc = start.ToUniversalTime();
            var endUtc = end.ToUniversalTime();

            using var client = _httpClientFactory.CreateClient();

            var getapxtariffsUri = new Uri(new Uri(_endpoint), "/nl/api/tariff/getapxtariffs");
            var queryString = new Dictionary<string, string?>
            {
                { "startTimestamp", startUtc.ToString("o") },
                { "endTimestamp", endUtc.ToString("o") }
            };

            var uri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(getapxtariffsUri.ToString(), queryString);

            using var resultStream = await client.GetStreamAsync(uri);
            var schema = JSon.GetSchema("EPEXSPOT.schemas.getapxtariffs.schema.json");

            // read the objects directly from the stream, including schema validation
            /*using JsonTextReader reader = new(new StreamReader(resultStream));
            JSchemaValidatingReader validatingReader = new(reader);
            validatingReader.Schema = GetSchema();
            IList<string> messages = new List<string>();
            validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

            JsonSerializer serializer = new() { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
            */
            using var streamReader = new StreamReader(resultStream);
            var result = JsonSerializer.Deserialize<SpotTariff[]>(streamReader.BaseStream, new JsonSerializerOptions());            
/*
            if (messages.Count > 0)
            {
                Logger.Error("There was a problem with the getapxtariffs response.");
                foreach (var m in messages)
                    Logger.Debug(m);
                return Array.Empty<Tariff>();
            }*/

            // calculate consumer price by adding the 'opslag duurzame energie', 'energie belasting' and 'inkoopkosten'
            var r = from x in result
                    select new Tariff(x.Timestamp, x.TariffUsage + ODE + EB + INKOOP, 0);

            // create array to return and make sure it is sorted
            var resultArray = r.ToArray();
            Array.Sort(resultArray, (x, y) => x.Timestamp.CompareTo(y.Timestamp));
            return resultArray;
        }

        public async Task<DateTime> GetTariffLastTimestamp()
        {
            using var client = _httpClientFactory.CreateClient();

            var getapxtariffslasttimestampUri = new Uri(new Uri(_endpoint), "/nl/api/tariff/getapxtariffslasttimestamp");
            using var resultStream = await client.GetStreamAsync(getapxtariffslasttimestampUri);
            using var streamReader = new StreamReader(resultStream);
            var result = JsonSerializer.Deserialize<DateTime>(streamReader.BaseStream, new JsonSerializerOptions());    

            //JsonSerializer serializer = new() { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
            //var result = serializer.Deserialize<DateTime>(reader);
            Logger.Info($"taris lasttime -> {result}");
            return result;
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

        private static Tariff[] RemoveOld(Tariff[] t)
        {
            var t2 = t.Where((x) => x.Timestamp > DateTime.UtcNow.AddHours(-2)).ToArray();
            Array.Sort(t2, (x, y) => { return x.Timestamp.CompareTo(y.Timestamp); });
            return t2;
        }

        private static JsonSchema GetSchema()
        {
            var schemaFile = "EPEXSPOT.schemas.getapxtariffs.schema.json";
           var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Logger.Trace($"looking for resources in {assembly.Location}");
            foreach (var s in assembly.GetManifestResourceNames())
            {
                Logger.Trace($"resource file {s}");
            }
            using Stream? resource = assembly.GetManifestResourceStream(schemaFile);
            if (resource == null) throw new FileNotFoundException($"Unable to load embedded resource {schemaFile}");

            using var r = new StreamReader(resource);
            var mySchema = JsonSchema.FromText(r.ReadToEnd());
            return mySchema;
        }
    }
}
