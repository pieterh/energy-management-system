using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AlfenNG9xx;
using EMS.Library;
using CommandLine;

namespace EMS
{
    static class Program
    {
        public class Options
        {
            [Option ('c', "config", Required = true, HelpText = "filename of config")]
            public string ConfigFile { get; set; }
            [Option('l', "nlogcfg", Required = false, HelpText = "filename of the nlog file")]
            public string nlogcfg { get; set; }
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static List<IBackgroundWorker> backgroundWorkers = new List<IBackgroundWorker>();
        static void Main(string[] args)
        {
            try
            {
                Logger.Info("Hello world");

                Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
                {
                    if (!string.IsNullOrWhiteSpace(o.nlogcfg))
                    {
                        //NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(o.nlogcfg);
                        NLog.LogManager.LoadConfiguration(o.nlogcfg);
                        NLog.LogManager.ReconfigExistingLoggers();
                        foreach (var t in NLog.LogManager.Configuration.AllTargets)
                        {
                            Logger.Info($"Available targets {t.Name}");
                        }
                    }

                    dynamic config = ConfigurationManager.ReadConfig(o.ConfigFile);

                    StartInstances(config);
                    var startTime = DateTime.Now;
                    while ((DateTime.Now - startTime).TotalMinutes <= (60 * 3))
                    {
                        Thread.Sleep(2500);
                    }

                    StopInstances();
                });
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static void StartInstances(dynamic config)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Logger.Trace($"looking for resources in {assembly.Location}");

            foreach (var instance in config.instances)
            {
                Logger.Info($"Instance [{instance.name}]");
                var adapter = GetAdapter(config, instance.adapterid);
                string assemblyFile = Path.Combine(Path.GetDirectoryName(assembly.Location), (string)adapter.driver.assembly);
                Logger.Info($"Instance [{instance.name}], loading instance from {assemblyFile}");
                var adapterAssembly = Assembly.LoadFrom(assemblyFile);
                var adapterType = adapterAssembly.GetType((string)adapter.driver.type);
                Logger.Info($"Instance [{instance.name}], type {adapterType.FullName} loaded");
                IBackgroundWorker adapterInstance = (IBackgroundWorker)Activator.CreateInstance(adapterType, (JObject)(instance.config));
                Logger.Info($"Instance [{instance.name}], created");
                backgroundWorkers.Add(adapterInstance);
                adapterInstance.Start();
                Logger.Info($"Instance [{instance.name}], started");
            }
        }

        static void StopInstances()
        {
            Console.WriteLine("== Stopping all background workers and waiting for them to finish. ==");
            var bgTasks = new List<Task>();
            backgroundWorkers.ForEach((backgroundWorker) =>
            {
                backgroundWorker.Stop();
                if (backgroundWorker.BackgroundTask != null)
                {
                    bgTasks.Add(backgroundWorker.BackgroundTask);
                }
            });

            try
            {
                Task.WaitAll(bgTasks.ToArray());
            }
            catch (System.AggregateException ae)
            {
                foreach (var ie in ae.InnerExceptions)
                {
                    if (typeof(System.OperationCanceledException) != ie.GetType())
                    {
                        Console.WriteLine($"There was a task with an error");
                    }
                }
            }
            Console.WriteLine("== Disposing all background workers ==");
            backgroundWorkers.ForEach((backgroundWorker) =>
            {
                backgroundWorker.Dispose();
            });
            backgroundWorkers.Clear();
        }

        public static dynamic GetAdapter(dynamic config, dynamic adapterid)
        {
            foreach (var adapter in config.adapters)
            {
                if (adapter.id == adapterid)
                    return adapter;
            }
            return null;
        }
    }
}
