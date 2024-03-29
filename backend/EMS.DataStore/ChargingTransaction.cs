﻿namespace EMS.DataStore;

public record ChargingTransaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public double EnergyDelivered { get; set; }
    public double Cost { get; set; }
    public double Price { get; set; }

    private ICollection<CostDetail>? _costDetails;
    public virtual ICollection<CostDetail> CostDetails
    {
        get
        {
            if (_costDetails == null) _costDetails = new List<CostDetail>();
            return _costDetails;
        }
    }

    protected virtual bool PrintMembers(StringBuilder stringBuilder)
    {
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));

        stringBuilder.Append($"ID = {ID}, ");
        stringBuilder.Append($"Timestamp = {Timestamp.ToLocalTime():O}, ");
        stringBuilder.Append($"EnergyDelivered = {EnergyDelivered} kWh, ");
        stringBuilder.Append($"Start = {Start.ToLocalTime():O}, ");
        stringBuilder.Append($"End = {End.ToLocalTime():O}, ");
        stringBuilder.Append($"Cost = €{Cost:F2}, ");
        stringBuilder.Append($"Price = €{Price:F2}");
        return true;
    }
}
