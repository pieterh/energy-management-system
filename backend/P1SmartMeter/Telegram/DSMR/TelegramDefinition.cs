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

        public const string PowerFailureEventLog = "1-0:99.97.0";

        public const string TextMessage = "0-0:96.13.0";

        public const string VoltageL1 = "1-0:32.7.0";
        public const string VoltageL2 = "1-0:52.7.0";
        public const string VoltageL3 = "1-0:72.7.0";

        public const string CurrentL1 = "1-0:31.7.0";
        public const string CurrentL2 = "1-0:51.7.0";
        public const string CurrentL3 = "1-0:71.7.0";

        private TelegramDefinition()                                        //NOSONAR
        {
            DefineField("1-3:0.2.8", "Version information", TelegramFieldType.String);
            DefineField(Timestamp, "Timestamp", TelegramFieldType.Timestamp);
            DefineField("0-0:96.1.1", "Equipment identifier", TelegramFieldType.String);
            DefineField(Electricity1ToClient, "Electricity Meter 1 - from grid");
            DefineField(Electricity2ToClient, "Electricity Meter 2 - from grid");
            DefineField(Electricity1FromClient, "Electricity Generated 1 - to grid");
            DefineField(Electricity2FromClient, "Electricity Generated 2 - to grid");
            DefineField(TariffIndicator, "Tariff indicator", TelegramFieldType.Numeric);
            DefineField(ActualPowerUse, "Actual power usage");
            DefineField(ActualPowerReturn, "Actual power return");
            DefineField("0-0:96.7.21", "Number of power failures", TelegramFieldType.Numeric);
            DefineField("0-0:96.7.9", "Number of long power failures", TelegramFieldType.Numeric);
            DefineField(PowerFailureEventLog, "Long power failure log", TelegramFieldType.Numeric, TelegramFieldType.Plain, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit, TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit);
            DefineField("1-0:32.32.0", "Num voltage sags L1", TelegramFieldType.Numeric);
            DefineField("1-0:52.32.0", "Num voltage sags L2", TelegramFieldType.Numeric);
            DefineField("1-0:72.32.0", "Num voltage sags L3", TelegramFieldType.Numeric);
            DefineField("1-0:32.36.0", "Num voltage swells L1", TelegramFieldType.Numeric);
            DefineField("1-0:52.36.0", "Num voltage swells L2", TelegramFieldType.Numeric);
            DefineField("1-0:72.36.0", "Num voltage swells L3", TelegramFieldType.Numeric);
            DefineField(TextMessage, "Text message", TelegramFieldType.String);
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

            for (var device = 1; device < 2; device++)
            {
                DefineField($"0-{device}:24.1.0", $"no {device}: Device type", TelegramFieldType.Numeric);
                DefineField($"0-{device}:96.1.0", $"no {device}: Equipment identifier", TelegramFieldType.String);
                DefineField(DeviceXLastReading(device), $"no {device}: Last reading timestamp and value", TelegramFieldType.Timestamp, TelegramFieldType.NumericWithUnit);
            }
        }
        public static TelegramDefinition Instance => new();

        public static string DeviceXLastReading(int x)
        {
            return $"0-{x}:24.2.1";
        }
    }
}
