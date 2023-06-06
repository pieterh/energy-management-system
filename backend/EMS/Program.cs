
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommandLine;
using NLog;
using NLog.Web;
using EMS.DataStore;
using EMS.DataStore.InMemory;
using EMS.Library;
using EMS.Library.Assembly;
using EMS.Library.Configuration;
using EMS.Library.Core;
using EMS.Library.dotNET;
using EMS.Library.Exceptions;
using EMS.Library.Files;
using EMS.WebHost;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Options;
using System.Threading;

namespace EMS;

internal sealed record Options
{
    [Option("config", Required = true, HelpText = "Filename of config.")]
    public string? ConfigFile { get; set; }
    [Option("nlogcfg", Required = false, HelpText = "Filename of the nlog file.")]
    public string? NLogConfig { get; set; }
    [Option("nlogdebug", Required = false, Default = false, HelpText = "Throw nlog internal exceptions. Not for production usage.")]
    public bool NLogDebug { get; set; }
    [Option("healthcheck", Required = false, Default = false, HelpText = "Checks health of a running instance")]
    public bool isHealthCheck { get; set; }
}

static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static NLog.Targets.ColoredConsoleTarget _logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole") { UseDefaultRowHighlightingRules = true };
    private static bool PerformHealthCheck;

    static async Task<int> Main(string[] args)
    {
        try
        {
            Thread.CurrentThread.Name = "HEMS main thread";
            EnforceLogging();

            Options options = new();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    options = o;
                })
                .WithNotParsed(errs =>
                {
                    throw new ProgramOptionsException("Something wrong with your arguments! ;-)");
                });

            PerformHealthCheck = options.isHealthCheck;

            ValidateConfigFile(options.ConfigFile);

            if (PerformHealthCheck)
            {                
                EnforceLoggingOnlyConsole();
                var hc = new HealthCheck(GetPortFromConfiguration(options));
                return await hc.PerformHealthCheck().ConfigureAwait(false);
            }

            ConfigureLogging(options);
            Logger.Info("============================================================");
            AssemblyInfo.Init();
            ResourceHelper.LogAllResourcesInAssembly(Assembly.GetExecutingAssembly());
            Logger.Info($"Git hash {ResourceHelper.ReadAsString(Assembly.GetExecutingAssembly(), "git.commit.hash.txt").Trim('\n', '\r')}");
            Logger.Info("============================================================");
            DotNetInfo.Info(Logger);
            Logger.Info("============================================================");
            Logger.Info("Garbage Collector Configuration");
            DotNetInfo.GCInfo(Logger);
            Logger.Info("============================================================");

            using var host = CreateHost(options);
            host.Run();
            return 0;
        }
        catch (ProgramOptionsException poe)
        {
            Logger.Error(poe.Message);
            return -1;
        }
        catch (ArgumentException ae)
        {
            /* Somehow we need to catch, otherwise the finally doesn't seem to run */
            Logger.Error(ae, "Unexpected error occurred and will terminate.");
            return -1;
        }
        finally
        {
            // keep healthcheck console output as clean as is possible
            if (!PerformHealthCheck)
                Logger.Info("=========================== DONE ===========================");
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
            _logconsole.Dispose();
        }
    }

    [SuppressMessage("GeneratedRegex", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Is executed only once for each run.")]
    private static int GetPortFromConfiguration(Options options)
    {
        var regexPort = new Regex(":(?<port>[0-9]+)", RegexOptions.None, new TimeSpan(0,0,0,0,250));
        KestrelConfig cfg = ConfigurationManager.ReadConfigProperty<KestrelConfig>(options.ConfigFile, "Kestrel");
        var m = regexPort.Matches(cfg.EndPoints?.Http?.Url ?? "");
        var port = m.SelectMany(y => y.Groups.Values).FirstOrDefault((x) => x.Name.Equals("port", StringComparison.Ordinal));
        NullException.ThrowIfNull(port, "Unable to retrieve port from configuration");
        return int.Parse(port.ValueSpan);
    }

    private static void ValidateConfigFile([NotNull] string? configFile)
    {
        if (!string.IsNullOrWhiteSpace(configFile) && FileTools.FileExistsAndReadable(configFile))
        {
            if (!ConfigurationManager.ValidateConfig(configFile))
                throw new ProgramOptionsException($"There was an error with the configuration file {configFile}");
        }
        else
        {
            throw new ProgramOptionsException($"The configuration file {configFile} was not found or not readable.");
        }
    }

    /// <summary>
    /// We need to make sure that if there is no logging configuration, that we will log to the console. This will
    /// ensure that there is always logging avaialable, even in the scenario of misconfiguration.
    /// Only basic logging will be provided to prevent excessive logging.
    /// </summary>
    [SuppressMessage("SonarLint", "S4792", Justification = "Ignored intentionally")]
    private static void EnforceLogging(bool overrideExisting = false)
    {
        if (LogManager.Configuration == null ||
            !LogManager.GetCurrentClassLogger().IsFatalEnabled || !LogManager.GetCurrentClassLogger().IsErrorEnabled ||
            overrideExisting)
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, _logconsole);
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();  // ensure that all existing loggers are updated with the new configuration
        }
    }

    /// <summary>
    /// Enforce only log to console
    /// Only info - fatel
    /// </summary>
    [SuppressMessage("SonarLint", "S4792", Justification = "Ignored intentionally")]
    private static void EnforceLoggingOnlyConsole()
    {
        var config = new NLog.Config.LoggingConfiguration();
        config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, _logconsole);
        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();  // ensure that all existing loggers are updated with the new configuration
    }

    private static void ConfigureLogging(Options options)
    {
        try
        {
            if (options.NLogDebug)
            {
                Logger.Warn("Enable NLog internal exception throwing. Do not use for production.");
                LogManager.ThrowConfigExceptions = true;
                LogManager.ThrowExceptions = true;
            }
            LogManager.Setup().LoadConfigurationFromFile(options.NLogConfig);
            LogManager.ReconfigExistingLoggers();
            if (!Logger.IsFatalEnabled || !Logger.IsErrorEnabled)
            {
                // set basic logging
                EnforceLogging(true);
                Logger.Fatal("No logging was configured. Default to basic logging to console.");
            }
            Logger.Info("Logger is configured.");
        }
        catch (FileNotFoundException)
        {
            EnforceLogging(true);
            Logger.Error($"The logger configuration file '{options.NLogConfig}' could not be found. Using default logging to console.");
        }
        catch (IOException e)
        {
            EnforceLogging(true);
            Logger.Error($"There was an io error during configuration or writing logfile. Using default logging to console.", e);
        }
    }

    static IHost CreateHost(Options options)
    {
        var configFile = options.ConfigFile;

        WebConfig wc = ConfigurationManager.ReadConfigProperty<WebConfig>(configFile, "web");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            ContentRootPath = wc.ContentRootPath,
            WebRootPath = wc.WebRootPath
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddNLogWeb();

        var env = builder.Environment;
        Logger.Info($"Hosting environment: {env.EnvironmentName}");
        var configFileEnvironmentSpecific = $"config.{env.EnvironmentName}.json";

        builder.Configuration.AddJsonFile(configFile, optional: false, reloadOnChange: false);
        if (FileTools.FileExistsAndReadable(configFileEnvironmentSpecific))
        {
            builder.Configuration.AddJsonFile(configFileEnvironmentSpecific, optional: true, reloadOnChange: false);
        }

        /* nog te doen: hmmm handle inital configuration in a better way and perform the creation or migration in a better way */
        DbConfig dbConfig = new();
        builder.Configuration.GetSection("db").Bind(dbConfig);
        HEMSContext.DbPath = dbConfig.name;
        using var a = new HEMSContext();
        a.Database.Migrate();

        builder.Services.AddDbContext<DataProtectionKeyContext>(o =>
        {
            o.UseInMemoryDatabase(DataProtectionKeyContext.DBName);
            o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        })
            .AddDataProtection()
            .AddKeyManagementOptions((opt) =>
            {
                opt.XmlEncryptor = new NullXmlEncryptor();
            })
            .PersistKeysToDbContext<DataProtectionKeyContext>();

        builder.Services.AddHttpClient();

        BackgroundServiceHelper.CreateAndStart<IWatchdog, Watchdog>(builder.Services);

        builder.ConfigureInstances();
        
        BackgroundServiceHelper.CreateAndStart<IHEMSCore, HEMSCore>(builder.Services);              

        // Create and configure startup
        var startup = new Startup(builder.Environment, builder.Configuration);
        startup.ConfigureServices(builder.Services);
        var app = builder.Build();

        startup.Configure(app);

        return app;
    }

    private static void ConfigureInstances(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var adapters = new List<AdapterConfiguration>();
        builder.Configuration.GetSection("adapters").Bind(adapters);

        var instances = new List<AdapterInstance>();
        builder.Configuration.GetSection("instances").Bind(instances);
        var activeInstances = instances.Where((x) => x.Enabled);

        foreach (var instance in activeInstances)
        {
            Logger.Debug($"Instance [{instance.Name}]");
            var adapter = GetAdapter(adapters, instance.AdapterId);
            if (adapter != null)
            {
                Logger.Debug($"Instance [{instance.Name}], loading assembly {adapter.Driver.Assembly}");

                var adapterAssembly = Assembly.Load(adapter.Driver.Assembly);
                Logger.Debug($"Instance [{instance.Name}], loaded assembly from location {adapterAssembly.Location}");

                var adapterType = adapterAssembly.GetType(adapter.Driver.Type);
                if (adapterType != null)
                {
                    Logger.Debug($"Instance [{instance.Name}], type {adapterType.FullName} loaded");

                    Logger.Debug($"Instance [{instance.Name}], configuring services");
                    const string methodName = "ConfigureServices";

                    var method = adapterType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { services, instance });
                        Logger.Debug($"Instance [InstanceName], configuring services done", instance.Name);
                    }
                    else
                    {
                        Logger.Error("The method {MethodName} was not found for adapter type {AdapeterType}", methodName, adapter.Driver.Type);
                    }
                }
                else
                    Logger.Error("The adapter type {AdapeterType} was not found", adapter.Driver.Type);
            }
            else
                Logger.Error("The adapter with id {Id} was not found", instance.AdapterId);
        }
    }

    public static AdapterConfiguration? GetAdapter(List<AdapterConfiguration> adapters, Guid adapterid)
    {
        foreach (var adapter in adapters)
        {
            if (adapter.Id == adapterid)
                return adapter;
        }
        return null;
    }


}
