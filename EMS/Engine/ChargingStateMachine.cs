using System;
namespace EMS.Engine
{
    public class ChargingStateMachine
    {
        public enum State { NotCharging, Charging, ChargingPaused }
        public State Current { get; set; } = State.NotCharging;
        public DateTime LastStateChange { get; set; } = DateTime.Now;
        public const int MINIMUM_TIME_SECS = 240;

        public State Start()
        {
            if (Current == State.NotCharging)
            {
                Current = State.Charging;
                LastStateChange = DateTime.Now;
            }
            return Current;
        }

        public State Stop()
        {
            if (Current != State.NotCharging)
            {
                Current = State.NotCharging;
                LastStateChange = DateTime.Now;
            }
            return Current;
        }

        public State Pause()
        {
            if (Current == State.Charging)
            {
                var secs = (DateTime.Now - LastStateChange).TotalSeconds;
                if (secs >= MINIMUM_TIME_SECS)
                {
                    Current = State.ChargingPaused;
                    LastStateChange = DateTime.Now;
                }
            }else
                if (Current == State.ChargingPaused)
                {
                    LastStateChange = DateTime.Now;
                }
            return Current;
        }

        public State Unpause()
        {
            if (Current == State.ChargingPaused)
            {
                var secs = (DateTime.Now - LastStateChange).TotalSeconds;
                if (secs >= MINIMUM_TIME_SECS)
                {
                    Current = State.Charging;
                    LastStateChange = DateTime.Now;
                }
            }else
            if (Current == State.Charging)
            {
                LastStateChange = DateTime.Now;
            }
            return Current;
        }
    }
}
