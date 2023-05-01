namespace EPEXSPOT;

public record SpotTariff
{
    public DateTime Timestamp { get; }
    public Decimal TariffUsage { get; }
    public Decimal TariffReturn { get; }

    public SpotTariff(DateTime timestamp, Decimal tariffUsage, Decimal tariffReturn)
    {
        Timestamp = timestamp.ToUniversalTime();
        TariffUsage = tariffUsage;
        TariffReturn = tariffReturn;
    }
}