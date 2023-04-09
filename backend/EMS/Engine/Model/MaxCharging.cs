using System;
using Microsoft.Extensions.Logging;

namespace EMS.Engine.Model
{
    public class MaxCharging : Base
    {
        protected static readonly NLog.Logger LoggerCurrent = NLog.LogManager.GetLogger("chargingcurrent");

        public override ushort MinimumDataPoints
        {
            get { return (ushort)15; }
        }

        public override ushort MaxBufferSeconds
        {
            get { return (ushort)60; }  // 15min or 1min buffer size
        }

        public MaxCharging(ILogger logger, Measurements measurements, ChargingStateMachine state) :
            base(logger, measurements, state)
        {

        }

        public override (double l1, double l2, double l3) GetCurrent()
        {

            var avg = Measurements.CalculateAverageUsage();

            if (avg.NrOfDataPoints < MinimumDataPoints) return (-1, -1, -1);
            LoggerCurrent.Info($"avg current {avg.CurrentUsingL1}, {avg.CurrentUsingL2} , {avg.CurrentUsingL3}");

            var retval1 = (float)Math.Round(LimitCurrent(avg.CurrentChargingL1, avg.CurrentUsingL1), 2);
            var retval2 = (float)Math.Round(LimitCurrent(avg.CurrentChargingL2, avg.CurrentUsingL2), 2);
            var retval3 = (float)Math.Round(LimitCurrent(avg.CurrentChargingL3, avg.CurrentUsingL3), 2);

            Logger?.LogInformation("{L1}, {L2}, {L3} => {Retval1}, {Retval2}, {Retval3}",
                (float)Math.Round(avg.CurrentUsingL1, 2), (float)Math.Round(avg.CurrentUsingL2, 2), (float)Math.Round(avg.CurrentUsingL3, 2),
                retval1, retval2, retval3);
            return (retval1, retval2, retval3);
        }

        private static double LimitCurrent(double c, double avgCurrentFromGrid)
        {
            var res = MaxCurrentMain + (c - avgCurrentFromGrid);
            double retval;
            if (res >= MaxCurrentChargePoint)
            {
                retval = MaxCurrentChargePoint;
            }
            else
            {
                retval = res <= MinimumChargeCurrent ? 0.0 : res;
            }
            return retval;
        }
    }
}
