using System;
using EMS.Library.Core;
using Microsoft.Extensions.Logging;

namespace EMS.Engine.Model
{
    public abstract class Base
    {
        protected static readonly NLog.Logger LoggerState = NLog.LogManager.GetLogger("chargingstate");
        protected const double MinimumChargeCurrent = 6.0f;            // IEC 61851 minimum current
        protected const double MaxCurrentMain = 25.0f;
        protected const double MaxCurrentChargePoint = 16.0f;

        protected readonly ILogger Logger;
        protected readonly Measurements _measurements;
        protected readonly ChargingStateMachine _state;

        public abstract ushort MinimumDataPoints { get; }
        public abstract ushort MaxBufferSeconds { get; }


        public Base(ILogger logger, Measurements measurements, ChargingStateMachine state)
        {
            Logger = logger;
            _measurements = measurements;
            _state = state;
        }

        public abstract (double l1, double l2, double l3) Get();

        protected (bool allow, bool changed) AllowToCharge()
        {
            bool stateHasChanged;
            if (_state.Current == ChargingState.Charging) return (true, false);

            if (_state.Current == ChargingState.ChargingPaused)
            {
                stateHasChanged = _state.Unpause();
                if (stateHasChanged)
                    LoggerState.Info($"Charging state unpaused");
            }
            else
            {
                stateHasChanged = _state.Start();
                LoggerState.Info($"Charging state start");
            }

            return (stateHasChanged, stateHasChanged);
        }
    }

    
}
