using System;
using EMS.Library.Core;
using Microsoft.Extensions.Logging;

namespace EMS.Engine.Model
{
    public class MaxSolar : Base
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

        public MaxSolar(ILogger logger, Measurements measurements, ChargingStateMachine state) :
            base(logger, measurements, state)
        {

        }

        public override (double l1, double l2, double l3) GetCurrent()
        {
            var avg = Measurements.CalculateAggregatedAverageUsage();

            if (avg.nrOfDataPoints < MinimumDataPoints) return (-1, -1, -1);
            Logger?.LogInformation("avg current {AverageUsage} and charging at {AverageCharge}", avg.averageUsage, avg.averageCharge);

            var chargeCurrent = Math.Round(LimitCurrentSolar(avg.averageCharge, avg.averageUsage), 2);

            bool stateHasChanged;
            if (chargeCurrent < MinimumChargeCurrent)
            {
                (chargeCurrent, stateHasChanged) = NotEnoughOverCapicity();
            }
            else
            {
                var t = AllowToCharge();
                if (t.changed)
                    LoggerCurrent?.Info("charging {chargeCurrent}", chargeCurrent);

                if (!t.allow)
                {
                    chargeCurrent = 0.0f;
                }
            }

            return ((float)Math.Round(chargeCurrent, 2), 0, 0);
        }

        private static double LimitCurrentSolar(double c, double avgCurrentFromGrid)
        {
            var res = c - avgCurrentFromGrid;

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

        private (double current, bool stateHasChanged) NotEnoughOverCapicity()
        {
            double retval;
            bool stateHasChanged = false;
            if (State.Current != ChargingState.NotCharging)
            {
                if (State.Current == ChargingState.ChargingPaused)
                {
                    retval = 0.0;
                    Logger?.LogInformation("Not enough solar power... We have stopped charging...");
                }
                else
                {
                    stateHasChanged = State.Pause();
                    if (stateHasChanged)
                    {
                        retval = 0.0;
                        Logger?.LogInformation("Not enough solar power... Stop charging......");
                    }
                    else
                    {
                        retval = MinimumChargeCurrent;
                        Logger?.LogInformation("Not enough solar power... Keep charging...");
                    }
                }
            }
            else
            {
                retval = 0.0;
            }

            return (current: retval, stateHasChanged: stateHasChanged);
        }
    }
}
