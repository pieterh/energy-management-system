﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommandLine;
using NLog.Extensions.Logging;
using EMS.Library.Configuration;

namespace EMS
{
    static class Program
    {
        public class Options
        {
            [Option('c', "config", Required = true, HelpText = "filename of config")]
            public string ConfigFile { get; set; }
            [Option('l', "nlogcfg", Required = false, HelpText = "filename of the nlog file")]
            public string NLogConfig { get; set; }
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            Options options = new();

            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                options = o;
            });

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

        static IHost CreateHost(Options options)
        {

            var t = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    if (ConfigurationManager.ValidateConfig(options.ConfigFile))
                    {
                        configuration.Sources.Clear();

                        IHostEnvironment env = hostingContext.HostingEnvironment;

                        configuration
                           .AddJsonFile("config.json", optional: false, reloadOnChange: false)
                           .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    }
                    else
                        Logger.Error("There was an error with the configuration file");

                })
            .ConfigureLogging((ILoggingBuilder logBuilder) =>
            {
                logBuilder.ClearProviders();
                logBuilder.SetMinimumLevel(LogLevel.Trace);
                logBuilder.AddNLog(options.NLogConfig);
            })
            .ConfigureServices((hostContext, services) =>
             {

                 services.Configure<List<Adapter>>(hostContext.Configuration.GetSection("adapters"));
                 services.Configure<List<EMS.Library.Configuration.Instance>>(hostContext.Configuration.GetSection("instances"));

                 ConfigureInstances(hostContext, services);

                 services.AddSingleton<IHostedService>(x => ActivatorUtilities.CreateInstance<HEMSCore>(x));
             }).Build();

            return t;
        }

        private static void ConfigureInstances(HostBuilderContext hostContext, IServiceCollection services)
        {
            var adapters = new List<Adapter>();
            hostContext.Configuration.GetSection("adapters").Bind(adapters);

            var instances = new List<Instance>();
            hostContext.Configuration.GetSection("instances").Bind(instances);

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
                                .Invoke(null, new object[] { hostContext, services, instance });
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


