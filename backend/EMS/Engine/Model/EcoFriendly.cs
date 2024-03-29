﻿using System;
using Microsoft.Extensions.Logging;
using EMS.Library.Core;
using EMS.Library.TestableDateTime;

namespace EMS.Engine.Model
{
    public class EcoFriendly : Base
    {
        public static double MinimumEcoModeExportStart { get { return 6.6f; } }
        public static double MinimumEcoModeExportStop { get { return 6.0f; } }

        public override ushort MinimumDataPoints
        {
            get { return (ushort)300; }
        }

        public override ushort MaxBufferSeconds
        {
            get { return (ushort)750; }     // 12,5 min buffer size
        }

        public EcoFriendly(ILogger logger, Measurements measurements, ChargingStateMachine state) :
            base(logger, measurements, state)
        {
        }

        public override (double l1, double l2, double l3) GetCurrent()
        {
            var avg = Measurements.CalculateAggregatedAverageUsage();

            if (avg.nrOfDataPoints < MinimumDataPoints) return (-1, -1, -1);

            var chargeCurrent = Math.Round(LimitEco(avg.averageCharge, avg.averageUsage), 2);
            Logger?.LogInformation("avg current {AverageUsage} and avg charging at {AverageCharge}; limitted chargecurrent = {ChargeCurrent} ({NrPoints} datapoints (buffer size {BufferSeconds} seconds)",
            avg.averageUsage, avg.averageCharge, chargeCurrent, avg.nrOfDataPoints, MaxBufferSeconds);

            if ((State.Current == ChargingState.Charging && chargeCurrent < MinimumEcoModeExportStop) ||
                (State.Current != ChargingState.Charging && chargeCurrent < MinimumEcoModeExportStart))
            {
                if (State.Current == ChargingState.Charging)
                {
                    State.Pause();
                    LoggerState.Info($"Charging state pause {chargeCurrent}");
                }

                return (0, 0, 0);
            }
            else
            {
                var t = AllowToCharge();
                if (t.allow)
                {
                    // charge as fast as possible and as close to the current available capicity as possible
                    var avgShort = Measurements.CalculateAggregatedAverageUsage(DateTimeProvider.Now.AddSeconds(-10));
                    var chargeCurrentShort1 = Math.Round(LimitEco(avgShort.averageCharge, avgShort.averageUsage), 2);

                    Logger?.LogInformation("charging {ChargeCurrent} -> {ChargeCurrentShort}", chargeCurrent, chargeCurrentShort1);
                    if (t.changed)
                        LoggerState?.Info("charging {chargeCurrent} -> {chargeCurrentShort}", chargeCurrent, chargeCurrentShort1);

                    return ((float)Math.Round(chargeCurrentShort1, 2), 0, 0);
                }
                else
                {
                    Logger?.LogInformation("charging {ChargeCurrent} -> not yet allowed", chargeCurrent);
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

            var chargeCurrentShort2 = retval - 0.15d; /* adjust 0.15A/ 35Wh just to be on the safe side*/
            var chargeCurrentShort3 = chargeCurrentShort2 >= MinimumChargeCurrent ? chargeCurrentShort2 : MinimumChargeCurrent;

            return chargeCurrentShort3;
        }
    }
}
