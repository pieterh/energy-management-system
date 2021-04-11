using System;
using EMS.Engine;
using FluentAssertions;
using Xunit;
using static EMS.Engine.ChargingStateMachine;

namespace EMS.Tests
{
    public class EngineStateMachine
    {
        const int MIN_DELAY = ChargingStateMachine.MINIMUM_TIME_SECS;
        const int DELAY = MIN_DELAY + 15;
        const int DELAY_SMALL = DELAY / 2;

        [Fact]
        public void Test1()
        {
            var stateMachine = new ChargingStateMachine
            {
                LastStateChange = DateTime.Now
            };

            stateMachine.Start().Should().Be(State.Charging, "we can start immediatly after creation");
            stateMachine.Stop().Should().Be(State.NotCharging, "we can stop immediatly after starting");
            stateMachine.Start().Should().Be(State.Charging, "we can start immediatly after stopping");
        }

        [Fact]
        public void Test2()
        {
            var stateMachine = new ChargingStateMachine
            {
                LastStateChange = DateTime.Now
            };

            stateMachine.Start().Should().Be(State.Charging, "we can start immediatly after creation");
            stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY);
            stateMachine.Pause().Should().Be(State.ChargingPaused, "after some time we should be able to pause");
            stateMachine.Stop().Should().Be(State.NotCharging, "we can stop immediatly even when paused");            
        }

        [Fact]
        public void Test3()
        {
            var stateMachine = new ChargingStateMachine
            {
                LastStateChange = DateTime.Now
            };

            stateMachine.Start().Should().Be(State.Charging, "we can start immediatly after creation");
            stateMachine.Pause().Should().Be(State.Charging, "we can't pause immediatly after starting");
            stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY_SMALL);
            stateMachine.Pause().Should().Be(State.Charging, "we can't pause immediatly after starting");

            stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY);
            stateMachine.Pause().Should().Be(State.ChargingPaused, "after some time we should be able to");
            stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY_SMALL);
            stateMachine.Unpause().Should().Be(State.ChargingPaused, "we can't unpause immediatly");
            stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY);
            stateMachine.Unpause().Should().Be(State.Charging, "after some time we should be able to");
        }
    }
}
