using System;
using System.Linq;
using EMS.Engine;
using EMS.Library;
using EMS.Library.Adapter;
using EMS.Library.Adapter.EVSE;
using EMS.Library.DateTimeProvider;
using Microsoft.Extensions.Logging;

namespace EMS
{
    public class Compute
    {
        private static readonly NLog.Logger LoggerState = NLog.LogManager.GetLogger("chargingstate");

        private readonly ILogger Logger;
        private readonly Measurements _measurements = new(60);        

        private ChargingMode _mode;
        public ChargingMode  Mode {
             get { return _mode; }
             set {
                _mode = value;
                // changing the mode also means that the buffer size is going to change
                _measurements.BufferSeconds = MaxBufferSeconds;
            }
        }

        public ushort MinimumDataPoints
        {
            get { return Mode == ChargingMode.MaxEco ? (ushort)300 : (ushort)15; }  
        }
        public ushort MaxBufferSeconds
        {
            get { return Mode == ChargingMode.MaxEco ? (ushort)900 : (ushort)60; }  // 15min or 1min buffer size
        }

        public ChargeControlInfo Info { get; private set; }
        private const double MinimumChargeCurrent = 6.0f;            // IEC 61851 minimum current
        private const double MaxCurrentMain = 25.0f;
        private const double MaxCurrentChargePoint = 16.0f;
        public const double MinimumEcoModeExport = 3.0f;

        private readonly ChargingStateMachine _state = new ();

        public Compute(ILogger logger, ChargingMode mode)
        {
            Logger = logger;
            Mode = mode;
            Info = new();
        }

        public (double l1, double l2, double l3) Charging()
        {
            switch (Mode)
            {
                case ChargingMode.MaxCharge:
                    var t = MaxCharging();
                    Info = new ChargeControlInfo(ChargingMode.MaxCharge, _state.Current, _state.LastStateChange, t.l1, t.l2, t.l3); 
                    return t;
                case ChargingMode.MaxEco:
                    var t2 = EcoMode();
                    return t2;                    
                case ChargingMode.MaxSolar:
                    var t3 =  MaxSolar();
                    Info = new ChargeControlInfo(ChargingMode.MaxSolar, _state.Current, _state.LastStateChange, t3.l1, t3.l2, t3.l3);
                    return t3;
                default:
                    return (-1, -1, -1);
            }
        }

        private (double l1, double l2, double l3) MaxCharging()
        {

            var avg = _measurements.CalculateAverageUsage();

            if (avg.NrOfDataPoints < MinimumDataPoints) return (-1, -1, -1);
            LoggerState.Info($"avg current {avg.CurrentUsingL1}, {avg.CurrentUsingL2} , {avg.CurrentUsingL3}");

            var retval1 = (float)Math.Round(LimitCurrent(avg.CurrentChargingL1, avg.CurrentUsingL1), 2);
            var retval2 = (float)Math.Round(LimitCurrent(avg.CurrentChargingL2, avg.CurrentUsingL2), 2);
            var retval3 = (float)Math.Round(LimitCurrent(avg.CurrentChargingL3, avg.CurrentUsingL3), 2);

            Logger?.LogInformation($"{(float)Math.Round(avg.CurrentUsingL1, 2)}, {(float)Math.Round(avg.CurrentUsingL2, 2)}, {(float)Math.Round(avg.CurrentUsingL3, 2)} => {retval1}, {retval2}, {retval3}");
            return (retval1, retval2, retval3);
        }

        private (double l1, double l2, double l3) EcoMode()
        {
            var avg = _measurements.CalculateAggregatedAverageUsage();            

            if (avg.nrOfDataPoints < MinimumDataPoints) return (-1, -1, -1);
            LoggerState.Info($"avg current {avg.averageUsage} and avg charging at {avg.averageCharge} with {avg.nrOfDataPoints} datapoints");
            Logger?.LogInformation($"avg current {avg.averageUsage} and avg charging at {avg.averageCharge} with {avg.nrOfDataPoints} datapoints");

            var chargeCurrent = Math.Round(LimitEco(avg.averageCharge, avg.averageUsage), 2);
            Logger?.LogInformation($"avg current {avg.averageUsage} and avg charging at {avg.averageCharge} with {avg.nrOfDataPoints} datapoints (buffer size {MaxBufferSeconds} seconds)");

            if (chargeCurrent < MinimumEcoModeExport)
            {
                bool stateHasChanged;
                (chargeCurrent, stateHasChanged) = NotEnoughOverCapicity();
                return ((float)Math.Round(chargeCurrent, 2), 0, 0);
            }
            else
            {
                Logger?.LogInformation($"{chargeCurrent}");
                if (AllowToCharge())                
                {
                    // charge as fast as possible and as close to the current available capicity as possible
                    var avgShort = _measurements.CalculateAggregatedAverageUsage(DateTimeProvider.Now.AddSeconds(-15));
                    var chargeCurrentShort = Math.Round(LimitEco(avgShort.averageCharge, avgShort.averageUsage), 2);
                    chargeCurrentShort = chargeCurrentShort >= MinimumChargeCurrent ? chargeCurrentShort : MinimumChargeCurrent;
                    Logger?.LogInformation($"charging {chargeCurrent} -> {chargeCurrentShort}");
                    return ((float)Math.Round(chargeCurrentShort, 2), 0, 0);
                }
                else
                {
                    return (0, 0, 0);
                }
            }
        }

        private (double l1, double l2, double l3) MaxSolar()
        {
            var avg = _measurements.CalculateAggregatedAverageUsage();

            if (avg.nrOfDataPoints < MinimumDataPoints) return (-1, -1, -1);
            LoggerState.Info($"avg current {avg.averageUsage} and charging at {avg.averageCharge}");

            var chargeCurrent = Math.Round(LimitCurrentSolar(avg.averageCharge, avg.averageUsage), 2);


            bool stateHasChanged;
            if (chargeCurrent < MinimumChargeCurrent)
            {
                (chargeCurrent, stateHasChanged) = NotEnoughOverCapicity();
            }
            else
            {
                if (!AllowToCharge())
                {
                    chargeCurrent = 0.0f;
                }
            }
                       
            return ((float)Math.Round(chargeCurrent, 2), 0, 0);
        }

        private bool AllowToCharge()
        {
            bool stateHasChanged;
            if (_state.Current == ChargingState.Charging) return true;

            if (_state.Current == ChargingState.ChargingPaused)
            {
                stateHasChanged = _state.Unpause();
            }
            else
            {
                stateHasChanged = _state.Start();
            }

            return stateHasChanged;
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
                        retval = 0.0;
                        Logger?.LogInformation($"Not enough solar power... Stop charging......");
                    }
                    else
                    {
                        retval = MinimumChargeCurrent;
                        Logger?.LogInformation($"Not enough solar power... Keep charging...");                        
                    }
                }
            }
            else
            {
                retval = 0.0;
            }

            return (current: retval, stateHasChanged: stateHasChanged);
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
                retval = res < MinimumEcoModeExport ? 0.0 : res;
            }
            return retval;
        }

        private static double LimitCurrentSolar(double c, double avgCurrentFromGrid)
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

        public void AddMeasurement(ICurrentMeasurement m, ICurrentMeasurement sm)
        {
            _measurements.AddData(m.CurrentL1, m.CurrentL2, m.CurrentL3, sm.CurrentL1, sm.CurrentL2, sm.CurrentL3);
        }
    }
}
