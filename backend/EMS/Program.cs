using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommandLine;
using NLog;
using NLog.Web;
using EMS.Library.Configuration;
using EMS.WebHost;
using EMS.Library;
using EMS.Library.Core;
using EMS.Library.Assembly;
using Microsoft.AspNetCore.Builder;

namespace EMS
{
    static class Program
    {
        public record Options
        {
            [Option('c', "config", Required = true, HelpText = "filename of config")]
            public string ConfigFile { get; set; }
            [Option('l', "nlogcfg", Required = false, HelpText = "filename of the nlog file")]
            public string NLogConfig { get; set; }
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            EnforceLogging();
            AssemblyInfo.Init();
            ResourceHelper.LogAllResourcesInAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            Logger.Info($"Git hash {ResourceHelper.ReadAsString(System.Reflection.Assembly.GetExecutingAssembly(), "EMS.git.commit.hash.txt")}");

            Options options = new();

            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                options = o;
            });

            ConfigureLogging(options);

            try
            {
                await CreateHost(options).RunAsync();
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        /// <summary>
        /// We need to make sure that if there is no logging configuration, that we will log to the console. This will
        /// ensure that there is always logging avaialable, even in the scenario of misconfiguration.
        /// </summary>
        private static void EnforceLogging()
        {
            if (Logger.Factory.Configuration == null)
            {                
                var config = new NLog.Config.LoggingConfiguration();
                var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
                logconsole.UseDefaultRowHighlightingRules = true;
                config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logconsole);
                NLog.LogManager.Configuration = config;
                Logger.Error("test");
            }
        }

        private static void ConfigureLogging(Options options)
        {
            Logger.Factory.LoadConfiguration(options.NLogConfig);
        }

        static IHost CreateHost(Options options)
        {           
            var t = Host.CreateDefaultBuilder()                
                .ConfigureLogging((ILoggingBuilder logBuilder) =>
                {
                    logBuilder.ClearProviders();
                })
                .UseNLog()
                .ConfigureAppConfiguration((builderContext, configuration) =>
                {
                    IHostEnvironment env = builderContext.HostingEnvironment;
                    Logger.Info($"Hosting environment: {env.EnvironmentName}");
                    if (ConfigurationManager.ValidateConfig(options.ConfigFile))
                    {
                        configuration.Sources.Clear();
                        configuration
                           .AddJsonFile(options.ConfigFile, optional: false, reloadOnChange: false)
                           .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    }
                    else
                        Logger.Error("There was an error with the configuration file");
                })
                .ConfigureServices((builderContext, services) =>
                {
                    services.AddHttpClient();
                    ConfigureInstances(builderContext, services);

                    BackgroundServiceHelper.CreateAndStart<IHEMSCore, HEMSCore>(services);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel((builderContext, kestrelOptions) =>
                    {

                        kestrelOptions.AddServerHeader = false;
                        WebConfig wc = new();
                        builderContext.Configuration.GetSection("web").Bind(wc);

                        kestrelOptions.ListenLocalhost(wc.Port, builder =>
                        {
                            if (!builderContext.HostingEnvironment.IsDevelopment())
                                builder.UseHttps();
                        });
                    });

                    webBuilder.UseStartup<Startup>();
                }).Build();
            
            return t;
        }

        private static void ConfigureInstances(HostBuilderContext hostingContext, IServiceCollection services)
        {
            var adapters = new List<Adapter>();
            hostingContext.Configuration.GetSection("adapters").Bind(adapters);

            var instances = new List<Instance>();
            hostingContext.Configuration.GetSection("instances").Bind(instances);

            foreach (var instance in instances)
            {
                Logger.Debug($"Instance [{instance.Name}]");
                var adapter = GetAdapter(adapters, instance.AdapterId);
                Logger.Debug($"Instance [{instance.Name}], loading assembly {adapter.Driver.Assembly}");

                var adapterAssembly = Assembly.Load(adapter.Driver.Assembly);
                Logger.Debug($"Instance [{instance.Name}], loaded assembly from location {adapterAssembly.Location}");

                var adapterType = adapterAssembly.GetType(adapter.Driver.Type);
                Logger.Debug($"Instance [{instance.Name}], type {adapterType.FullName} loaded");

                Logger.Debug($"Instance [{instance.Name}], configuring services");
                adapterType.GetMethod("ConfigureServices", BindingFlags.Static | BindingFlags.Public)
                                .Invoke(null, new object[] { hostingContext, services, instance });
                Logger.Debug($"Instance [{instance.Name}], configuring services done");
            }
        }

        public static Adapter GetAdapter(List<Adapter> adapters, Guid adapterid)
        {
            foreach (var adapter in adapters)
            {
                if (adapter.Id == adapterid)
                    return adapter;
            }
            return null;
        }
    }

}


