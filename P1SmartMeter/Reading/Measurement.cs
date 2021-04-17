using System;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter.Reading
{
    public class Measurement : EMS.Library.Measurement
    {
        public Measurement(DSMRTelegram t)
        {
            Timestamp = t.Timestamp;
            TariffIndicator = t.TariffIndicator;

            var powerL1 = ConvertPower(t.PowerUsedL1, t.PowerReturnedL1);
            var powerL2 = ConvertPower(t.PowerUsedL2, t.PowerReturnedL2);
            var powerL3 = ConvertPower(t.PowerUsedL3, t.PowerReturnedL3);

            // calculate the current based on power and voltage to get a better accuracy
            CurrentL1 = CalculateCurrent(powerL1, t.VoltageL1);
            CurrentL2 = CalculateCurrent(powerL2, t.VoltageL2);
            CurrentL3 = CalculateCurrent(powerL3, t.VoltageL3);

            VoltageL1 = t.VoltageL1;
            VoltageL2 = t.VoltageL2;
            VoltageL3 = t.VoltageL3;

            Electricity1FromGrid = t.Electricity1FromGrid;
            Electricity1ToGrid = t.Electricity1ToGrid;
            Electricity2FromGrid = t.Electricity2FromGrid;
            Electricity2ToGrid = t.Electricity2ToGrid;
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
