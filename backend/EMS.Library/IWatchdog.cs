namespace EMS.Library;

public interface IWatchdog
{
    void Register(IBackgroundWorker bgWorkerToWatch, int interval);
    void Unregister(IBackgroundWorker bgWorkerUnwatch);
    void Tick(IBackgroundWorker bgWorkerTicked);

    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}