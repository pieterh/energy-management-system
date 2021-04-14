using System;
namespace EMS.Engine
{
    public class ChargingStateMachine
    {
        //private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("chargingstate");

        public enum State { NotCharging, Charging, ChargingPaused }
        public State Current { get; set; } = State.NotCharging;
        public DateTime LastStateChange { get; set; } = DateTime.Now;
        public const int MINIMUM_TIME_SECS = 240;

        public bool Start()
        {
            bool stateHasChanged = false;
            if (Current == State.NotCharging)
            {
                UpdateState(State.Charging);
                stateHasChanged = true;
            }
            return stateHasChanged;
        }

        public bool Stop()
        {
            bool stateHasChanged = false;
            if (Current != State.NotCharging)
            {
                UpdateState(State.NotCharging);
                stateHasChanged = true;
            }
            return stateHasChanged;
        }

        public bool Pause()
        {
            bool stateHasChanged = false;
            if (Current == State.Charging)
            {
                var secs = (DateTime.Now - LastStateChange).TotalSeconds;
                if (secs >= MINIMUM_TIME_SECS)
                {
                    UpdateState(State.ChargingPaused);
                    stateHasChanged = true;
                }
            }else
                if (Current == State.ChargingPaused)
                {
                    LastStateChange = DateTime.Now;
                }
            return stateHasChanged;
        }

        public bool Unpause()
        {
            bool stateHasChanged = false;
            if (Current == State.ChargingPaused)
            {
                var secs = (DateTime.Now - LastStateChange).TotalSeconds;
                if (secs >= MINIMUM_TIME_SECS)
                {
                    UpdateState(State.Charging);
                    stateHasChanged = true;
                }
            }else
            if (Current == State.Charging)
            {
                LastStateChange = DateTime.Now;
            }
            return stateHasChanged;
        }

        private void UpdateState(State newState)
        {
            //Logger.Info($"Change state {Current} -> {newState}");
            Current = newState;
            LastStateChange = DateTime.Now;
        }
    }
}
