using System;
using EMS.Library.TestableDateTime;
using Microsoft.Extensions.Logging;

namespace EMS.Engine.Model
{
    public class SlowCharge : Base
    {
        public override ushort MinimumDataPoints
        {
            get { return (ushort)10; }
        }

        public override ushort MaxBufferSeconds
        {
            get { return (ushort)60; }  // 15min or 1min buffer size
        }

        public SlowCharge(ILogger logger, Measurements measurements, ChargingStateMachine state) :
            base(logger, measurements, state)
        {
        }

        public override (double l1, double l2, double l3) Get()
        {
            if (_measurements.ItemsInBuffer < MinimumDataPoints) return (-1, -1, -1);

            var avgShort = _measurements.CalculateAggregatedAverageUsage(DateTimeProvider.Now.AddSeconds(-10));
            var chargeCurrentShort1 = Math.Round(LimitCurrent(avgShort.averageCharge, avgShort.averageUsage), 2);
                        
            return ((float)Math.Round(chargeCurrentShort1, 2), 0, 0);
        }

        private static double LimitCurrent(double c, double avgCurrentFromGrid)
        {
            var res = c - avgCurrentFromGrid;
            res -= 0.15; /* adjust 0.15A/ 35Wh just to be on the safe side*/
            double retval = res <= MinimumChargeCurrent ? MinimumChargeCurrent : res;
            return retval;
        }
    }
}
