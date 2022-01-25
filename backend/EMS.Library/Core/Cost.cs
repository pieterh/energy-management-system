using System;
using EMS.Library.Adapter.PriceProvider;

namespace EMS.Library.Core
{
	public record Cost
	{
		public DateTime Timestamp { get; set; }
		public Tariff Tariff { get; set; }
		public Decimal Energy { get; set; }
		public Cost(DateTime timestamp, Tariff tarif, Decimal energy)
		{
			Timestamp = timestamp;
			Tariff = tarif;
			Energy = energy;
		}
		public Cost(DateTime timestamp, Tariff tarif, double energy)
		{
			Timestamp = timestamp;
			Tariff = tarif;
			Energy = (Decimal)energy;
		}
	}
}

