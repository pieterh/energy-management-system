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
        public enum ChargingMode { MaxCharge, MaxSolar };

        private static readonly NLog.Logger LoggerState = NLog.LogManager.GetLogger("chargingstate");

        private readonly ILogger Logger;
        public ChargingMode Mode { get; set; } = ChargingMode.MaxCharge;

        private readonly Measurements _measurements = new();

        public const ushort MinimumDataPoints = 10;

        private const float MinimumChargeCurrent = 6.0f;        // IEC 61851 minimum current
        private const float MaxCurrentMain = 25.0f;
        private const float MaxCurrentChargePoint = 16.0f;

        private readonly ChargingStateMachine _state = new ();

        public Compute(ILogger logger, ChargingMode mode)
        {
            Logger = logger;
            Mode = mode;
        }

        public (float, float, float) Charging(ChargingInfo ci)
        {
            switch (Mode)
            {
                case ChargingMode.MaxCharge:
                    return MaxCharging(ci);                
                case ChargingMode.MaxSolar:
                    return MaxSolar(ci);
                default:
                    return (-1, -1, -1);
            }
        }

        private (float, float, float) MaxCharging(ChargingInfo ci)
        {
            var m = _measurements.Get();
            if (ci == null || m.Length < MinimumDataPoints) return ( -1, -1, -1);

            var avgCurrentUsingL1 = (float)Math.Round(m.Average(x => x.CurrentL1).Value, 2);
            var avgCurrentUsingL2 = (float)Math.Round(m.Average(x => x.CurrentL2).Value, 2);
            var avgCurrentUsingL3 = (float)Math.Round(m.Average(x => x.CurrentL3).Value, 2);

            float retval1 = (float)Math.Round(LimitCurrent(ci.CurrentL1, avgCurrentUsingL1), 2);
            float retval2 = (float)Math.Round(LimitCurrent(ci.CurrentL2, avgCurrentUsingL2), 2);
            float retval3 = (float)Math.Round(LimitCurrent(ci.CurrentL3, avgCurrentUsingL3), 2);
            Logger?.LogInformation($"{avgCurrentUsingL1}, {avgCurrentUsingL2}, {avgCurrentUsingL3} => {retval1}, {retval2}, {retval3}");
            return (retval1, retval2, retval3);
        }

        private (float, float, float) MaxSolar(ChargingInfo ci)
        {
            var m = _measurements.Get();
            if (ci == null || m.Length < MinimumDataPoints) return (-1, -1, -1);

            var stateHasChanged = false;

            var avgPowerL1 = (float)m.Average(x => x.PowerL1).Value;
            var avgPowerL2 = (float)m.Average(x => x.PowerL2).Value;
            var avgPowerL3 = (float)m.Average(x => x.PowerL3).Value;

            float retval1 = (float)Math.Round(LimitCurrentSolar(ci.CurrentL1, ci.VoltageL1, avgPowerL1), 2);
            float retval2 = (float)Math.Round(LimitCurrentSolar(ci.CurrentL2, ci.VoltageL2, avgPowerL2), 2);
            float retval3 = (float)Math.Round(LimitCurrentSolar(ci.CurrentL3, ci.VoltageL3, avgPowerL3), 2);
            
            if (retval1 < MinimumChargeCurrent)
            {
                if (_state.Current != ChargingStateMachine.State.NotCharging)
                {                   
                    if (stateHasChanged = _state.Pause())
                    {
                        retval1 = 0.0f;
                        Logger?.LogInformation($"Not enough solar power... Stop charging...");
                        //stateHasChanged = true;
                    }
                    else
                    {
                        Logger?.LogInformation($"Not enough solar power... Keep charging...");
                        retval1 = MinimumChargeCurrent;
                    }
                }
                else
                {
                    retval1 = 0.0f;
                }
            }
            else {
                if (_state.Current == ChargingStateMachine.State.ChargingPaused)
                {
                    stateHasChanged = _state.Unpause();
                }
                else
                {
                    stateHasChanged = _state.Start();
                }
                if (_state.Current != ChargingStateMachine.State.Charging)
                {
                    retval1 = 0.0f;
                }
            }

            Logger?.LogInformation($"{avgPowerL1}, {avgPowerL2}, {avgPowerL3} => {retval1}, {retval2}, {retval3}, {stateHasChanged}, {_state.Current}");
            if (stateHasChanged)
            {
                LoggerState.Info($"State changed {_state.Current}");
            }

            return (retval1, 0f, 0f);
        }

        private static float LimitCurrent(float c, float avgCurrentUsing)
        {
            float res = c + (MaxCurrentMain - avgCurrentUsing);
            float retval;
            if (res >= MaxCurrentChargePoint)
            {
                retval = MaxCurrentChargePoint;
            }
            else
            {
                retval = res <= MinimumChargeCurrent ? 0 : res;
            }
            return retval;
        }

        private static float LimitCurrentSolar(float c, float u, float avgPower)
        {
            float current = (avgPower * 1000.0f) / u;

            float res = c + current;
            float retval;
            if (res >= MaxCurrentChargePoint) {
                retval = MaxCurrentChargePoint;
            } else {
                retval = res <= MinimumChargeCurrent ? 0.0f : res;
            }
            return retval;
        }

        public void AddMeasurement(MeasurementBase m)
        {
            _measurements.Add(m);
        }
    }
}
