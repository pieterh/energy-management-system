using Microsoft.Extensions.Hosting;
using EMS.Library.Adapter.SmartMeterAdapter;

namespace EMS.Library.Adapter.Solar;

public interface ISolar : IAdapter, IHostedService
{
    Task<bool> StopProduction(CancellationToken cancellationToken);
    Task<bool> StartProduction(CancellationToken cancellationToken);
    Task<bool> GetProductionStatus(CancellationToken cancellationToken);
}