using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMS.DataStore;

public record CostDetail
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int ID { get; set; }
	public DateTime Timestamp { get; set; }
	public double EnergyDelivered { get; set; }
	public double Cost { get; set; }
	public DateTime TarifStart { get; set; }
	public double TarifUsage { get; set; }

	protected virtual bool PrintMembers(StringBuilder stringBuilder)
	{
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));

        stringBuilder.Append($"ID = {ID}, ");
		stringBuilder.Append($"Timestamp = {Timestamp.ToLocalTime():O}, ");
		stringBuilder.Append($"EnergyDelivered = {EnergyDelivered} kWh, ");
		stringBuilder.Append($"Cost = €{Cost:F2}, ");
		stringBuilder.Append($"TarifStart = {TarifStart.ToLocalTime():O}, ");
		stringBuilder.Append($"TarifUsage = €{TarifUsage:F2}, ");
		return true;
	}
}
