using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AlfenNG9xx;

namespace EMS
{
    static class Program
    {
        private static List<IBackgroundWorker> backgroundWorkers = new List<IBackgroundWorker>();
        static void Main(string[] args)
        {
            var alfen = new AlfenNG9xx.AlfenNG9xx();
            backgroundWorkers.Add(alfen);

            alfen.Start();
            Thread.Sleep(10000);

            StopBackgroundWorkers();
        }

        static void StopBackgroundWorkers()
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
    }
}
