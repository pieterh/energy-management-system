using Microsoft.Extensions.DependencyInjection;

namespace EMS.Library;

public interface IWatchdog
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bgWorkerToWatch"></param>
    /// <param name="interval">expected interval in seconds</param>
    void Register(IBackgroundWorker bgWorkerToWatch, int interval);
    void Unregister(IBackgroundWorker bgWorkerUnwatch);
    void Tick(IBackgroundWorker bgWorkerTicked);

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}



