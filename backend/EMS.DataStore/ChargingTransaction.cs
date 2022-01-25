using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMS.DataStore;
public record ChargingTransaction
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int ID { get; set; }
    public DateTime Timestamp { get; set; }
    public double EnergyDelivered { get; set; }
    public double Cost { get; set; }
    public double Price { get; set; }

    private ICollection<CostDetail>? _costDetails;
    public virtual ICollection<CostDetail> CostDetails {
        get { return _costDetails ??= _costDetails = new List<CostDetail>() ; }
        set { _costDetails = value ; }
    }


    protected virtual bool PrintMembers(StringBuilder stringBuilder)
    {
        stringBuilder.Append($"ID = {ID}, ");
        stringBuilder.Append($"Timestamp = {Timestamp.ToLocalTime():O}, ");
        stringBuilder.Append($"EnergyDelivered = {EnergyDelivered} kWh, ");
        stringBuilder.Append($"Cost = €{Cost:F2}, ");
        stringBuilder.Append($"Price = €{Price:F2}");
        return true;
    }
}

