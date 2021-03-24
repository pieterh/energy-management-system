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
            PowerUsingL1 = t.PowerUsedL1;
            PowerUsingL2 = t.PowerUsedL2;
            PowerUsingL3 = t.PowerUsedL3;
            PowerReturningL1 = t.PowerReturnedL1;
            PowerReturningL2 = t.PowerReturnedL2;
            PowerReturningL3 = t.PowerReturnedL3;

            Electricity1FromGrid = t.Electricity1FromGrid;
            Electricity1ToGrid = t.Electricity1ToGrid;
            Electricity2FromGrid = t.Electricity2FromGrid;
            Electricity2ToGrid = t.Electricity2ToGrid;
        }
    }
}
