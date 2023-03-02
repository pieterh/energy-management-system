using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EMS.Library.Adapter.PriceProvider
{
	public interface IPriceProvider : IAdapter, IHostedService
	{
		/// <summary>
        /// Returns an array containing the tariff for the given timespan.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
		Task<Tariff[]> GetTariff(DateTime start, DateTime end);

        /// <summary>
        /// Returns the current tariff
        /// </summary>
        /// <returns></returns>
        Tariff? GetTariff();
    }
}

