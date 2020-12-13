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

namespace EMS
{
    static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static List<IBackgroundWorker> backgroundWorkers = new List<IBackgroundWorker>();
        static void Main(string[] args)
        {
            Logger.Info("Hello world");
            var c = new ConfigurationManager();
            dynamic config = c.ReadConfig();

            StartInstances(config);

            Thread.Sleep(10000);

            StopInstances();
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
                IBackgroundWorker adapterInstance = (IBackgroundWorker)Activator.CreateInstance(adapterType, (JObject)(config.instances[0].config));
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
