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

        private readonly ILogger _logger;
        private readonly Measurements _measurements;
        private readonly ChargingStateMachine _state;

        public abstract ushort MinimumDataPoints { get; }
        public abstract ushort MaxBufferSeconds { get; }

        protected ILogger Logger => _logger;

        protected Measurements Measurements => _measurements;

        protected ChargingStateMachine State => _state;

        protected Base(ILogger logger, Measurements measurements, ChargingStateMachine state)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
            _measurements = measurements;
            _state = state;
        }

        public abstract (double l1, double l2, double l3) GetCurrent();

        protected (bool allow, bool changed) AllowToCharge()
        {
            bool stateHasChanged;
            if (State.Current == ChargingState.Charging) return (true, false);

            if (State.Current == ChargingState.ChargingPaused)
            {
                stateHasChanged = State.Unpause();
                if (stateHasChanged)
                    LoggerState.Info($"Charging state unpaused");
            }
            else
            {
                stateHasChanged = State.Start();
                LoggerState.Info($"Charging state start");
            }

            return (stateHasChanged, stateHasChanged);
        }
    }    
}
