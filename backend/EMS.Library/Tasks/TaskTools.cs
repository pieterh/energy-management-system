using System;
namespace EMS.Library.Tasks
{
    public static class TaskTools
    {
        /// <summary>
        /// Waits for the task to complete execution within the specified number of milliseconds.
        /// Wrapper around Task.Wait and ignores AggregateException if the inner exceptions only contain TaskCanceledException exceptions
        /// </summary>
        /// <param name="task"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static bool Wait(Task? task, int millisecondsTimeout)
        {
            return Wait(task, millisecondsTimeout, CancellationToken.None);
        }

        /// <summary>
        /// Waits for the task to complete execution within the specified number of milliseconds.
        /// Wrapper around Task.Wait and ignores AggregateException if the inner exceptions only contain TaskCanceledException exceptions
        /// </summary>
        /// <param name="task"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>true if the task is finished on time, false otherwise</returns>
        public static bool Wait(Task ?task, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            /* wait a bit for the background task in the case that it still is trying to connect */
            try
            {
                return task?.Wait(millisecondsTimeout, cancellationToken) ?? false;                
            }
            catch (AggregateException e) when (e.InnerExceptions.Any((x) => x is not TaskCanceledException))
            {
                /* throw if any of the inner exceptions if not of type TaskCanceledException*/
                throw;
            }
            catch (AggregateException)
            {
                /* Intentially ignoring */
            }
            catch (OperationCanceledException)
            {
                /* Intentially ignoring */
            }
            return false;
        }
    }
}

