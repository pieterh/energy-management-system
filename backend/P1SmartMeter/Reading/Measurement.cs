using System;
using EMS.Library.Adapter.SmartMeterAdapter;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter.Reading
{
    public record Measurement : SmartMeterMeasurementBase
    {
        public Measurement(DSMRTelegram telegram)
        {
            ArgumentNullException.ThrowIfNull(telegram);
            Timestamp = telegram.Timestamp;
            TariffIndicator = telegram.TariffIndicator;

            var powerL1 = ConvertPower(telegram.PowerUsedL1, telegram.PowerReturnedL1);
            var powerL2 = ConvertPower(telegram.PowerUsedL2, telegram.PowerReturnedL2);
            var powerL3 = ConvertPower(telegram.PowerUsedL3, telegram.PowerReturnedL3);

            // calculate the current based on power and voltage to get a better accuracy
            CurrentL1 = CalculateCurrent(powerL1, telegram.VoltageL1);
            CurrentL2 = CalculateCurrent(powerL2, telegram.VoltageL2);
            CurrentL3 = CalculateCurrent(powerL3, telegram.VoltageL3);

            VoltageL1 = telegram.VoltageL1;
            VoltageL2 = telegram.VoltageL2;
            VoltageL3 = telegram.VoltageL3;

            Electricity1FromGrid = telegram.Electricity1FromGrid;
            Electricity1ToGrid = telegram.Electricity1ToGrid;
            Electricity2FromGrid = telegram.Electricity2FromGrid;
            Electricity2ToGrid = telegram.Electricity2ToGrid;
        }

        private static double? ConvertPower(double? powerUsed, double? powerReturned)
        {
            double? retval;
            if (powerUsed.HasValue && powerUsed.Value > 0)
            {
                retval = powerUsed;
            }
            else
            {
                retval = powerReturned.HasValue ? -powerReturned : null;
            }
            return retval;
        }

        private static double? CalculateCurrent(double? power, double? voltage)
        {
            if (!power.HasValue || !voltage.HasValue) return null;
            var retval = Math.Round((power.Value / voltage.Value) * 1000, 2, MidpointRounding.AwayFromZero);
            return retval;
        }
    }
}
