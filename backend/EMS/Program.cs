﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
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
using EMS.DataStore.InMemory;
using EMS.Library;
using EMS.Library.Assembly;
using EMS.Library.Configuration;
using EMS.Library.Core;

using EMS.WebHost;

using EMS.DataStore;

namespace EMS
{
    static class Program
    {
        public record Options
        {
            [Option("config", Required = true, HelpText = "Filename of config.")]
            public string ConfigFile { get; set; }
            [Option("nlogcfg", Required = false, HelpText = "Filename of the nlog file.")]
            public string NLogConfig { get; set; }
            [Option("nlogdebug", Required = false, Default = false, HelpText = "Throw nlog internal exceptions. Not for production usage.")]
            public bool NLogDebug { get; set; }
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static NLog.Targets.ColoredConsoleTarget _logconsole;

        static void Main(string[] args)
        {
            try
            {
                _logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
                _logconsole.UseDefaultRowHighlightingRules = true;

                EnforceLogging();

                Options options = new();

                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(o =>
                    {
                        options = o;
                    })
                    .WithNotParsed(errs => {
                        throw new ArgumentException("Something wrong with your arguments! ;-)");
                    });
         

                ConfigureLogging(options);
                Logger.Info("============================================================");
                AssemblyInfo.Init();

                ResourceHelper.LogAllResourcesInAssembly(System.Reflection.Assembly.GetExecutingAssembly());
                Logger.Info($"Git hash {ResourceHelper.ReadAsString(System.Reflection.Assembly.GetExecutingAssembly(), "git.commit.hash.txt").Trim('\n', '\r')}");

                using var host = CreateHost(options);
                host.Run();
            }
            catch (ArgumentException) { /* Somehow we need to catch, otherwise the finally doesn't seem to run */ }
            finally
            {                
                Logger.Info("========================= DONE ==============================");
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();                
                _logconsole.Dispose();                
            }
        }

        /// <summary>
        /// We need to make sure that if there is no logging configuration, that we will log to the console. This will
        /// ensure that there is always logging avaialable, even in the scenario of misconfiguration.
        /// Only basic logging will be provided to prevent excessive logging.
        /// </summary>
        private static void EnforceLogging(bool overrideExisting = false)
        {
            if (NLog.LogManager.Configuration == null ||
                !NLog.LogManager.GetCurrentClassLogger().IsFatalEnabled || !NLog.LogManager.GetCurrentClassLogger().IsErrorEnabled ||
                overrideExisting)
            {                
                var config = new NLog.Config.LoggingConfiguration();
                config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, _logconsole);
                NLog.LogManager.Configuration = config; // Sensitive
                NLog.LogManager.ReconfigExistingLoggers();  // ensure that all existing loggers are updated with the new configuration
            }
        }

        private static void ConfigureLogging(Options options)
        {
            try
            {
                if (options.NLogDebug)
                {
                    Logger.Warn("Enable NLog internal exception throwing. Do not use for production.");
                    NLog.LogManager.ThrowConfigExceptions = true;
                    NLog.LogManager.ThrowExceptions = true;
                }
                NLog.LogManager.LoadConfiguration(options.NLogConfig);
                NLog.LogManager.ReconfigExistingLoggers();
                if (!Logger.IsFatalEnabled || !Logger.IsErrorEnabled)
                {
                    // set basic logging
                    EnforceLogging(true);
                    Logger.Fatal("No logging was configured. Default to basic logging to console.");
                }                
            }catch(FileNotFoundException)
            {
                Logger.Error($"Ther logger configuration file '{options.NLogConfig}' could not be found. Using default logging to console.");
                EnforceLogging(true);
            }
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
                    /* nog te doen: hmmm handle inital configuration in a better way and perform the creation or migration in a better way */
                    EMS.DataStore.DbConfig dbConfig = new();
                    builderContext.Configuration.GetSection("db").Bind(dbConfig);
                    EMS.DataStore.HEMSContext.DbPath = dbConfig.name;
                    using var a = new HEMSContext();
                    a.Database.Migrate();

                    services
                        .AddDbContext<DataProtectionKeyContext>(o => {
                                o.UseInMemoryDatabase(DataProtectionKeyContext.DBName);
                                o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                        })
                        .AddDataProtection()
                        .AddKeyManagementOptions((opt) => {
                            opt.XmlEncryptor = new NullXmlEncryptor();
                            })
                        .PersistKeysToDbContext<DataProtectionKeyContext>();

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
                        kestrelOptions.ListenAnyIP(wc.Port, builder =>
                        {
                            if (wc.https && !builderContext.HostingEnvironment.IsDevelopment())
                            {
                                Logger.Warn("Using https is currently not supported.");
                                builder.UseHttps();
                            }
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
