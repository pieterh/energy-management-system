using System;
using EMS.Library.Core;
using EMS.Library.TestableDateTime;
using Microsoft.Extensions.Logging;

namespace EMS.Engine.Model
{
    public class EcoFriendly: Base
    {
        public const double MinimumEcoModeExportStart = 6.0f;
        public const double MinimumEcoModeExportStop = 4.0f;

        public override ushort MinimumDataPoints
        {
            get { return  (ushort)300 ; }
        }

        public override ushort MaxBufferSeconds
        {
            get { return  (ushort)900; }  // 15min or 1min buffer size
        }

        public EcoFriendly(ILogger logger, Measurements measurements, ChargingStateMachine state) :
            base(logger, measurements, state)
        {

        }

        public override (double l1, double l2, double l3) Get()
        {
            var avg = _measurements.CalculateAggregatedAverageUsage();

            if (avg.nrOfDataPoints < MinimumDataPoints) return (-1, -1, -1);

            //Logger?.LogInformation($"avg current {avg.averageUsage} and avg charging at {avg.averageCharge} with {avg.nrOfDataPoints} datapoints");

            var chargeCurrent = Math.Round(LimitEco(avg.averageCharge, avg.averageUsage), 2);
            Logger?.LogInformation($"avg current {avg.averageUsage} and avg charging at {avg.averageCharge}; limitted chargecurrent = {chargeCurrent} ({avg.nrOfDataPoints} datapoints (buffer size {MaxBufferSeconds} seconds)");

            if ((_state.Current == ChargingState.Charging && chargeCurrent < MinimumEcoModeExportStop) ||
                (_state.Current != ChargingState.Charging && chargeCurrent < MinimumEcoModeExportStart))
            {
                if (_state.Current == ChargingState.Charging)
                {
                    _state.Pause();
                    LoggerState.Info($"Charging state pause {chargeCurrent}");
                }

                chargeCurrent = 0.0;

                return (0, 0, 0);
            }
            else
            {
                //Logger?.LogInformation($"{chargeCurrent}");
                var t = AllowToCharge();
                if (t.allow)
                {
                    // charge as fast as possible and as close to the current available capicity as possible
                    var avgShort = _measurements.CalculateAggregatedAverageUsage(DateTimeProvider.Now.AddSeconds(-10));
                    var chargeCurrentShort1 = Math.Round(LimitEco(avgShort.averageCharge, avgShort.averageUsage), 2);
                    var chargeCurrentShort2 = chargeCurrentShort1 - 0.15; /* adjust 0.15A/ 35Wh just to be on the safe side*/
                    var chargeCurrentShort3 = chargeCurrentShort2 >= MinimumChargeCurrent ? chargeCurrentShort2 : MinimumChargeCurrent;
                    Logger?.LogInformation($"charging {chargeCurrent} -> {chargeCurrentShort1} -> {chargeCurrentShort2} -> -> {chargeCurrentShort3}");
                    if (t.changed)
                        LoggerState?.Info($"charging {chargeCurrent} -> {chargeCurrentShort1} -> {chargeCurrentShort2} -> -> {chargeCurrentShort3}");

                    return ((float)Math.Round(chargeCurrentShort3, 2), 0, 0);
                }
                else
                {
                    return (0, 0, 0);
                }
            }
        }

        private static double LimitEco(double c, double avgCurrentFromGrid)
        {
            var res = c - avgCurrentFromGrid;

            double retval;
            if (res >= MaxCurrentChargePoint)
            {
                retval = MaxCurrentChargePoint;
            }
            else
            {
                retval = res < MinimumEcoModeExportStop ? 0.0 : res;
            }
            return retval;
        }
    }
}
