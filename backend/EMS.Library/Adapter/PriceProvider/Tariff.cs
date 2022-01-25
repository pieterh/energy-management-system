using System;
namespace EMS.Library.Adapter.PriceProvider
{
	public record Tariff
	{
		public DateTime Timestamp { get; }
		public Decimal TariffUsage { get; }
		public Decimal TariffReturn { get; }

		public Tariff(DateTime timestamp, Decimal tariffUsage, Decimal tariffReturn)
		{
			Timestamp = timestamp;
			TariffUsage = tariffUsage;
			TariffReturn = tariffReturn;
		}
	}
}

