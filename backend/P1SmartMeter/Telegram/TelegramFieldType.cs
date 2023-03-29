using System.Diagnostics.CodeAnalysis;

namespace P1SmartMeter.Telegram
{
    [SuppressMessage("Roslyn", "CA1720", Justification = "Ignored intentionally")]
    public enum TelegramFieldType
    {
        NumericWithUnit,
        NumericWithUnitAsDouble,
        Numeric,
        String,
        OctetString,
        Timestamp,
        Plain
    }
}
