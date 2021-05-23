using System;
using System.Linq;
using EMS.Engine;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using Microsoft.Extensions.Logging;

namespace EMS
{
    public class Compute
    {
        private static readonly NLog.Logger LoggerState = NLog.LogManager.GetLogger("chargingstate");

        private readonly ILogger Logger;
        public ChargingMode Mode { get; set; }

        public ChargeControlInfo Info { get; private set; }

        private readonly Measurements _measurements = new();

        public const ushort MinimumDataPoints = 10;

        private const double MinimumChargeCurrent = 6.0f;            // IEC 61851 minimum current
        private const double MaxCurrentMain = 25.0f;
        private const double MaxCurrentChargePoint = 16.0f;

        private readonly ChargingStateMachine _state = new ();

        public Compute(ILogger logger, ChargingMode mode)
        {
            Logger = logger;
            Mode = mode;
            Info = new();
        }

        public (double l1, double l2, double l3) Charging(ChargingInfo ci)
        {
            switch (Mode)
            {
                case ChargingMode.MaxCharge:
                    var t = MaxCharging(ci);
                    Info = new ChargeControlInfo(ChargingMode.MaxCharge, _state.Current, _state.LastStateChange, t.l1, t.l2, t.l3); 
                    return t;
                case ChargingMode.MaxSolar:
                    var t2 =  MaxSolar(ci);
                    Info = new ChargeControlInfo(ChargingMode.MaxSolar, _state.Current, _state.LastStateChange, t2.l1, t2.l2, t2.l3);
                    return t2;
                default:
                    return (-1, -1, -1);
            }
        }

        private (double l1, double l2, double l3) MaxCharging(ChargingInfo ci)
        {
            var m = _measurements.Get();
            if (ci == null || m.Length < MinimumDataPoints) return ( -1, -1, -1);

            var avgCurrentUsingL1 = m.Average(x => x.CurrentL1).Value;
            var avgCurrentUsingL2 = m.Average(x => x.CurrentL2).Value;
            var avgCurrentUsingL3 = m.Average(x => x.CurrentL3).Value;

            var retval1 = (float)Math.Round(LimitCurrent(ci.CurrentL1, avgCurrentUsingL1), 2);
            var retval2 = (float)Math.Round(LimitCurrent(ci.CurrentL2, avgCurrentUsingL2), 2);
            var retval3 = (float)Math.Round(LimitCurrent(ci.CurrentL3, avgCurrentUsingL3), 2);

            Logger?.LogInformation($"{(float)Math.Round(avgCurrentUsingL1, 2)}, {(float)Math.Round(avgCurrentUsingL2, 2)}, {(float)Math.Round(avgCurrentUsingL3, 2)} => {retval1}, {retval2}, {retval3}");
            return (retval1, retval2, retval3);
        }

        private (double l1, double l2, double l3) MaxSolar(ChargingInfo ci)
        {
            var m = _measurements.Get();
            if (ci == null || m.Length < MinimumDataPoints) return (-1, -1, -1);

            var stateHasChanged = false;

            var avgCurrentUsingL1 = m.Average(x => x.CurrentL1).Value;
            var avgCurrentUsingL2 = m.Average(x => x.CurrentL2).Value;
            var avgCurrentUsingL3 = m.Average(x => x.CurrentL3).Value;

            var avgCurrent = avgCurrentUsingL1 + avgCurrentUsingL2 + avgCurrentUsingL3;

            var chargeCurrent = Math.Round(LimitCurrentSolar(ci.CurrentL1, avgCurrent), 2);
            
            if (chargeCurrent < MinimumChargeCurrent)
            {
                (chargeCurrent, stateHasChanged) = NotEnoughOverCapicity();
            } else {
                if (_state.Current == ChargingState.ChargingPaused)
                {
                    stateHasChanged = _state.Unpause();
                }
                else
                {
                    stateHasChanged = _state.Start();
                }

                if (_state.Current != ChargingState.Charging)
                {
                    chargeCurrent = 0.0;
                }
            }

            Logger?.LogInformation($"{(float)Math.Round(avgCurrentUsingL1, 2)}, {(float)Math.Round(avgCurrentUsingL2, 2)}, {(float)Math.Round(avgCurrentUsingL3, 2)} => {chargeCurrent}, {stateHasChanged}, {_state.Current}");

            if (stateHasChanged)
            {
                LoggerState.Info($"State changed {_state.Current}");
            }
            
            return ((float)Math.Round(chargeCurrent, 2), 0, 0);
        }

        private (double current, bool stateHasChanged) NotEnoughOverCapicity()
        {
            double retval;
            bool stateHasChanged = false;
            if (_state.Current != ChargingState.NotCharging)
            {
                if (_state.Current == ChargingState.ChargingPaused)
                {
                    retval = 0.0;
                    Logger?.LogInformation($"Not enough solar power... We have stopped charging...");
                }
                else
                {
                    stateHasChanged = _state.Pause();
                    if (stateHasChanged)
                    {
                        // TODO: uh?
                        retval = 0.0;
                        Logger?.LogInformation($"Not enough solar power... Stop charging......");
                    }

                    Logger?.LogInformation($"Not enough solar power... Keep charging...");
                    retval = MinimumChargeCurrent;
                }
            }
            else
            {
                retval = 0.0;
            }

            return (current: retval, stateHasChanged: stateHasChanged);
        }

        private static double LimitCurrent(double c, double avgCurrentUsing)
        {
            var res = c + (MaxCurrentMain - avgCurrentUsing);
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

        private static double LimitCurrentSolar(float c, double avgCurrentFromGrid)
        {
            var res = c - avgCurrentFromGrid;

            double retval;
            if (res >= MaxCurrentChargePoint) {
                retval = MaxCurrentChargePoint;
            } else {
                retval = res <= MinimumChargeCurrent ? 0.0 : res;
            }
            return retval;
        }

        public void AddMeasurement(MeasurementBase m)
        {
            _measurements.Add(m);
        }
    }
}
