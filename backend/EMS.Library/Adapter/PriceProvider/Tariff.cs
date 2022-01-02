using System;
namespace EMS.Library.Adapter.PriceProvider
{
	public record Tariff
	{
		public DateTime Timestamp { get; }
		public double TariffUsage { get; }
		public double TariffReturn { get; }

		public Tariff(DateTime timestamp, double tariffUsage, double tariffReturn)
		{
			Timestamp = timestamp;
			TariffUsage = tariffUsage;
			TariffReturn = tariffReturn;
		}
	}
}

