using System;

namespace P1SmartMeter.Telegram.DSMR
{
    public class DSMRTelegram : TelegramBase
    {
        public DSMRTelegram(string raw) : base(TelegramDefinition.Instance, raw)
        {
            Timestamp = GetValue<DateTime?>(TelegramDefinition.Timestamp);
            Electricity1FromGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity1ToClient)?.Item1;
            Electricity2FromGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity2ToClient)?.Item1;
            Electricity1ToGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity1FromClient)?.Item1;
            Electricity2ToGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity2FromClient)?.Item1;

            PowerUsedL1 = GetValue<(double, string)?>(TelegramDefinition.PowerUsedL1)?.Item1;
            PowerUsedL2 = GetValue<(double, string)?>(TelegramDefinition.PowerUsedL2)?.Item1;
            PowerUsedL3 = GetValue<(double, string)?>(TelegramDefinition.PowerUsedL3)?.Item1;

            PowerReturnedL1 = GetValue<(double, string)?>(TelegramDefinition.PowerReturnedL1)?.Item1;
            PowerReturnedL2 = GetValue<(double, string)?>(TelegramDefinition.PowerReturnedL2)?.Item1;
            PowerReturnedL3 = GetValue<(double, string)?>(TelegramDefinition.PowerReturnedL3)?.Item1;

            TariffIndicator = GetValue<int?>(TelegramDefinition.TariffIndicator);
            ActualPower = GetValue<(double, string)?>(TelegramDefinition.ActualPower)?.Item1;
            TextMessage = GetValue<string>(TelegramDefinition.TextMessage);
        }

        public DateTime? Timestamp { get; }

        public string TextMessage { get; }

        public double? ActualPower { get; }

        public int? TariffIndicator { get; }

        public double? Electricity1FromGrid { get; }

        public double? Electricity2FromGrid { get; }

        public double? Electricity1ToGrid { get; }

        public double? Electricity2ToGrid { get; }

        public double? PowerUsedL1 { get; }
        public double? PowerUsedL2 { get; }
        public double? PowerUsedL3 { get; }

        public double? PowerReturnedL1 { get; }
        public double? PowerReturnedL2 { get; }
        public double? PowerReturnedL3 { get; }

        public override string ToString()
        {
            return $"Timestamp: {Timestamp}\nE1: {Electricity1FromGrid}\nE2: {Electricity1FromGrid}\nTariff: {TariffIndicator}\nActualPower: {ActualPower}\nTextMessage: {TextMessage}";
        }
    }
}
