using System;

namespace P1SmartMeter.Telegram.DSMR
{
    public class TelegramDefinition : TelegramDefinitionBase
    {
        // some important fields
        public const string Timestamp = "0-0:1.0.0";
        public const string Electricity1ToClient = "1-0:1.8.1";
        public const string Electricity2ToClient = "1-0:1.8.2";
        public const string Electricity1FromClient = "1-0:2.8.1";
        public const string Electricity2FromClient = "1-0:2.8.2";

        public const string PowerUsedL1 = "1-0:21.7.0";
        public const string PowerUsedL2 = "1-0:41.7.0";
        public const string PowerUsedL3 = "1-0:61.7.0";
        public const string PowerReturnedL1 = "1-0:22.7.0";
        public const string PowerReturnedL2 = "1-0:42.7.0";
        public const string PowerReturnedL3 = "1-0:62.7.0";

        public const string TariffIndicator = "0-0:96.14.0";
        public const string ActualPowerUse = "1-0:1.7.0";
        public const string ActualPowerReturn = "1-0:2.7.0";
        public const string PowerFailures = "0-0:96.7.21";
        public const string PowerFailuresLong = "0-0:96.7.9";
        public const string PowerFailureEventLog = "1-0:99.97.0";
        public const string PowerSagsL1 = "1-0:32.32.0";
        public const string PowerSagsL2 = "1-0:52.32.0";
        public const string PowerSagsL3 = "1-0:72.32.0";
        public const string PowerSwellsL1 = "1-0:32.36.0";
        public const string PowerSwellsL2 = "1-0:52.36.0";
        public const string PowerSwellsL3 = "1-0:72.36.0";
        public const string TextMessage = "0-0:96.13.0";
        public const string VoltageL1 = "1-0:32.7.0";
        public const string VoltageL2 = "1-0:52.7.0";
        public const string VoltageL3 = "1-0:72.7.0";

        public const string CurrentL1 = "1-0:31.7.0";
        public const string CurrentL2 = "1-0:51.7.0";
        public const string CurrentL3 = "1-0:71.7.0";

        public const string MBUSClient1Type = "0-1:24.1.0";
        public const string MBUSClient1Ident = "0-1:96.1.0";
        public const string MBUSClient1Measurement = "0-1:24.2.1";
        public const string MBUSClient2Type = "0-2:24.1.0";
        public const string MBUSClient2Ident = "0-2:96.1.0";
        public const string MBUSClient2Measurement = "0-2:24.2.1";
        public const string MBUSClient3Type = "0-3:24.1.0";
        public const string MBUSClient3Ident = "0-3:96.1.0";
        public const string MBUSClient3Measurement = "0-3:24.2.1";
        public const string MBUSClient4Type = "0-4:24.1.0";
        public const string MBUSClient4Ident = "0-4:96.1.0";
        public const string MBUSClient4Measurement = "0-4:24.2.1";

        public static MBusClientFields GetMBusClientFields(int index)
        {
            switch (index)
            {
                case 1: return new MBusClientFields(MBUSClient1Type, MBUSClient1Ident, MBUSClient1Measurement);
                case 2: return new MBusClientFields(MBUSClient1Type, MBUSClient1Ident, MBUSClient1Measurement);
                case 3: return new MBusClientFields(MBUSClient1Type, MBUSClient1Ident, MBUSClient1Measurement);
                case 4: return new MBusClientFields(MBUSClient1Type, MBUSClient1Ident, MBUSClient1Measurement);
                default: throw new ArgumentException("Value must between 1 and 4", nameof(index));
            }
        }

        private TelegramDefinition()
        {
            DefineField("1-3:0.2.8", "Version information", TelegramFieldType.String);
            DefineField(Timestamp, "Timestamp", TelegramFieldType.Timestamp);
            DefineField("0-0:96.1.1", "Equipment identifier", TelegramFieldType.OctetString);
            DefineField(Electricity1ToClient, "Electricity Meter 1 - from grid");
            DefineField(Electricity2ToClient, "Electricity Meter 2 - from grid");
            DefineField(Electricity1FromClient, "Electricity Generated 1 - to grid");
            DefineField(Electricity2FromClient, "Electricity Generated 2 - to grid");
            DefineField(TariffIndicator, "Tariff indicator", TelegramFieldType.Numeric);
            DefineField(ActualPowerUse, "Actual power usage");
            DefineField(ActualPowerReturn, "Actual power return");
            DefineField(PowerFailures, "Number of power failures", TelegramFieldType.Numeric);
            DefineField(PowerFailuresLong, "Number of long power failures", TelegramFieldType.Numeric);
            DefineField(PowerFailureEventLog, "Long power failure log", TelegramFieldType.Numeric, TelegramFieldType.Plain, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit);
            DefineField(PowerSagsL1, "Num voltage sags L1", TelegramFieldType.Numeric);
            DefineField(PowerSagsL2, "Num voltage sags L2", TelegramFieldType.Numeric);
            DefineField(PowerSagsL3, "Num voltage sags L3", TelegramFieldType.Numeric);
            DefineField(PowerSwellsL1, "Num voltage swells L1", TelegramFieldType.Numeric);
            DefineField(PowerSwellsL2, "Num voltage swells L2", TelegramFieldType.Numeric);
            DefineField(PowerSwellsL3, "Num voltage swells L3", TelegramFieldType.Numeric);
            DefineField(TextMessage, "Text message", TelegramFieldType.OctetString);
            DefineField(VoltageL1, "Voltage L1");
            DefineField(VoltageL2, "Voltage L2");
            DefineField(VoltageL3, "Voltage L3");
            DefineField(CurrentL1, "Current L1");
            DefineField(CurrentL2, "Current L2");
            DefineField(CurrentL3, "Current L3");
            DefineField(PowerUsedL1, "Power Used L1", TelegramFieldType.NumericWithUnit);
            DefineField(PowerUsedL2, "Power Used L2", TelegramFieldType.NumericWithUnit);
            DefineField(PowerUsedL3, "Power Used L3", TelegramFieldType.NumericWithUnit);
            DefineField(PowerReturnedL1, "Power Returned L1", TelegramFieldType.NumericWithUnit);
            DefineField(PowerReturnedL2, "Power Returned L2", TelegramFieldType.NumericWithUnit);
            DefineField(PowerReturnedL3, "Power Returned L3", TelegramFieldType.NumericWithUnit);

            DefineField(MBUSClient1Type, "M-Bus client 1 - device type", TelegramFieldType.Numeric);
            DefineField(MBUSClient1Ident, "M-Bus client 1 - identifier", TelegramFieldType.String);
            DefineField(MBUSClient1Measurement, "M-Bus client 1 - measurement", TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnitAsDouble);

            DefineField(MBUSClient2Type, "M-Bus client 2 - device type", TelegramFieldType.Numeric);
            DefineField(MBUSClient2Ident, "M-Bus client 2 - identifier", TelegramFieldType.String);
            DefineField(MBUSClient2Measurement, "M-Bus client 2 - measurement", TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnitAsDouble);

            DefineField(MBUSClient3Type, "M-Bus client 3 - device type", TelegramFieldType.Numeric);
            DefineField(MBUSClient3Ident, "M-Bus client 3 - identifier", TelegramFieldType.String);
            DefineField(MBUSClient3Measurement, "M-Bus client 3 - measurement", TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnitAsDouble);

            DefineField(MBUSClient4Type, "M-Bus client 4 - device type", TelegramFieldType.Numeric);
            DefineField(MBUSClient4Ident, "M-Bus client 4 - identifier", TelegramFieldType.String);
            DefineField(MBUSClient4Measurement, "M-Bus client 4 - measurement", TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnitAsDouble);
        }
        public static TelegramDefinition Instance => new();

        public static string DeviceXLastReading(int x)
        {
            return $"0-{x}:24.2.1";
        }
    }
    public struct MBusClientFields : IEquatable<MBusClientFields>
    {
        public string Type { get; }
        public string Ident { get; }
        public string Measurement { get; }
        public MBusClientFields(string t, string i, string m)
        {
            Type = t;
            Ident = i;
            Measurement = m;
        }

        public override bool Equals(object obj)
        {
            if (obj is not MBusClientFields) return false;
            var other = (MBusClientFields)obj;
            return Equals(other);
        }

        public bool Equals(MBusClientFields other)
        {
            return this.Type.Equals(other.Type, StringComparison.Ordinal) &&
                   this.Ident.Equals(other.Ident, StringComparison.Ordinal) &&
                   this.Measurement.Equals(other.Measurement, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return String.Concat(Type, Ident, Measurement).GetHashCode(StringComparison.Ordinal);
        }

        public static bool operator ==(MBusClientFields left, MBusClientFields right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MBusClientFields left, MBusClientFields right)
        {
            return !(left == right);
        }
    }
}
