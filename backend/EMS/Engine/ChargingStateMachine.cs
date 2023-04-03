using System;
using System.Diagnostics.CodeAnalysis;
using EMS.Library;
using EMS.Library.Core;
using EMS.Library.TestableDateTime;

namespace EMS.Engine
{
    public class ChargingStateMachine
    {

        public ChargingState Current { get; set; } = ChargingState.NotCharging;
        public DateTime LastStateChange { get; set; } = DateTimeProvider.Now;
        [SuppressMessage("Code Analysis","CA1707")]
        public const int DEFAULT_MINIMUM_TRANSITION_TIME = (6*60); // seconds

        public int MinimumTransitionTime { get; set; }

        public ChargingStateMachine()
        {
            MinimumTransitionTime = DEFAULT_MINIMUM_TRANSITION_TIME;
        }

        public bool Start()
        {
            bool stateHasChanged = false;
            if (Current == ChargingState.NotCharging)
            {
                UpdateState(ChargingState.Charging);
                stateHasChanged = true;
            }
            return stateHasChanged;
        }

        public bool Stop()
        {
            bool stateHasChanged = false;
            if (Current != ChargingState.NotCharging)
            {
                UpdateState(ChargingState.NotCharging);
                stateHasChanged = true;
            }
            return stateHasChanged;
        }

        public bool Pause()
        {
            bool stateHasChanged = false;
            if (Current == ChargingState.Charging)
            {
                var secs = (DateTimeProvider.Now - LastStateChange).TotalSeconds;
                if (secs >= MinimumTransitionTime)
                {
                    UpdateState(ChargingState.ChargingPaused);
                    stateHasChanged = true;
                }
            }
            else
            {
                if (Current == ChargingState.ChargingPaused)
                {
                    LastStateChange = DateTimeProvider.Now;
                }
            }
            return stateHasChanged;
        }

        public bool Unpause()
        {
            bool stateHasChanged = false;
            if (Current == ChargingState.ChargingPaused)
            {
                var secs = (DateTimeProvider.Now - LastStateChange).TotalSeconds;
                if (secs >= MinimumTransitionTime)
                {
                    UpdateState(ChargingState.Charging);
                    stateHasChanged = true;
                }
            }
            else
            {
                if (Current == ChargingState.Charging)
                {
                    LastStateChange = DateTimeProvider.Now;
                }
            }
            return stateHasChanged;
        }

        private void UpdateState(ChargingState newState)
        {
            Current = newState;
            LastStateChange = DateTimeProvider.Now;
        }
    }
}
