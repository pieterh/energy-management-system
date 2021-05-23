using System;
using EMS.Library;

namespace EMS.Engine
{
    public class ChargingStateMachine
    {

        public ChargingState Current { get; set; } = ChargingState.NotCharging;
        public DateTime LastStateChange { get; set; } = DateTime.Now;
        public const int MINIMUM_TIME_SECS = 240;

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
                var secs = (DateTime.Now - LastStateChange).TotalSeconds;
                if (secs >= MINIMUM_TIME_SECS)
                {
                    UpdateState(ChargingState.ChargingPaused);
                    stateHasChanged = true;
                }
            }
            else
            {
                if (Current == ChargingState.ChargingPaused)
                {
                    LastStateChange = DateTime.Now;
                }
            }
            return stateHasChanged;
        }

        public bool Unpause()
        {
            bool stateHasChanged = false;
            if (Current == ChargingState.ChargingPaused)
            {
                var secs = (DateTime.Now - LastStateChange).TotalSeconds;
                if (secs >= MINIMUM_TIME_SECS)
                {
                    UpdateState(ChargingState.Charging);
                    stateHasChanged = true;
                }
            }
            else
            {
                if (Current == ChargingState.Charging)
                {
                    LastStateChange = DateTime.Now;
                }
            }
            return stateHasChanged;
        }

        private void UpdateState(ChargingState newState)
        {
            Current = newState;
            LastStateChange = DateTime.Now;
        }
    }
}
