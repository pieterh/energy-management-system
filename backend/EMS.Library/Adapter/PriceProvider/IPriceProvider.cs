using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EMS.Library.Adapter.PriceProvider
{
	public interface IPriceProvider : IAdapter, IHostedService
	{
        /// <summary>
        /// Returns the current tariff
        /// </summary>
        /// <returns></returns>
        Tariff? GetTariff();

        /// <summary>
        /// Returns the tariff for the next hour
        /// </summary>
        /// <returns></returns>
        Tariff? GetNextTariff();
    }
}

