using EMS.Library.Adapter.PriceProvider;

namespace EMS.Library.Core
{
	public record Cost
	{
		public DateTimeOffset Timestamp { get; set; }
		public Tariff? Tariff { get; set; }
		public Decimal Energy { get; set; }
		public Cost(DateTimeOffset timestamp, Tariff tarif, Decimal energy)
		{
			Timestamp = timestamp;
			Tariff = tarif;
			Energy = energy;
		}
		public Cost(DateTimeOffset timestamp, Tariff? tarif, double energy)
		{
			Timestamp = timestamp;
			Tariff = tarif;
			Energy = (Decimal)energy;
		}
	}
}
