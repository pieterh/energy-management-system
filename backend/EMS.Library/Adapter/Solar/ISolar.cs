using Microsoft.Extensions.Hosting;
using EMS.Library.Adapter.SmartMeterAdapter;

namespace EMS.Library.Adapter.Solar;

public interface ISolar : IAdapter, IHostedService
{
    Task<bool> StopProduction();
    Task<bool> StartProduction();
    Task<bool> GetProductionStatus();
}