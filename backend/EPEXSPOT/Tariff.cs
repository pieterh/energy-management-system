namespace EPEXSPOT;

public record SpotTariff
{
    public DateTime Timestamp { get; }
    public double TariffUsage { get; }
    public double TariffReturn { get; }

    public SpotTariff(DateTime timestamp, double tariffUsage, double tariffReturn)
    {
        Timestamp = timestamp;
        TariffUsage = tariffUsage;
        TariffReturn = tariffReturn;
    }
}
