using System;
using EMS.Engine;
using EMS.Library;
using EMS.Library.Core;
using EMS.Library.TestableDateTime;
using FluentAssertions;
using Xunit;
using static EMS.Engine.ChargingStateMachine;

namespace EMS.Tests
{
    public class EngineStateMachine
    {
        private readonly int MIN_DELAY;
        private readonly int DELAY;
        private readonly int DELAY_SMALL;
        private readonly ChargingStateMachine _stateMachine;

        public EngineStateMachine()
        {
            _stateMachine = new ChargingStateMachine
            {
                LastStateChange = DateTimeProvider.Now
            };

            MIN_DELAY = _stateMachine.MinimumTransitionTime;
            DELAY = MIN_DELAY + 15;
            DELAY_SMALL = DELAY / 2;
        }

        [Fact]
        public void Test1()
        {
            _stateMachine.Start();
            _stateMachine.Current.Should().Be(ChargingState.Charging, "we can start immediatly after creation");
            _stateMachine.Stop();
            _stateMachine.Current.Should().Be(ChargingState.NotCharging, "we can stop immediatly after starting");
            _stateMachine.Start();
            _stateMachine.Current.Should().Be(ChargingState.Charging, "we can start immediatly after stopping");
        }

        [Fact]
        public void Test2()
        {
            _stateMachine.Start();
            _stateMachine.Current.Should().Be(ChargingState.Charging, "we can start immediatly after creation");
            _stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY);
            _stateMachine.Pause();
            _stateMachine.Current.Should().Be(ChargingState.ChargingPaused, "after some time we should be able to pause");
            _stateMachine.Stop();
            _stateMachine.Current.Should().Be(ChargingState.NotCharging, "we can stop immediatly even when paused");            
        }

        [Fact]
        public void Test3()
        {
            _stateMachine.Start();
            _stateMachine.Current.Should().Be(ChargingState.Charging, "we can start immediatly after creation");
            _stateMachine.Pause();
            _stateMachine.Current.Should().Be(ChargingState.Charging, "we can't pause immediatly after starting");
            _stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY_SMALL);
            _stateMachine.Pause();
            _stateMachine.Current.Should().Be(ChargingState.Charging, "we can't pause immediatly after starting");

            _stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY);
            _stateMachine.Pause();
            _stateMachine.Current.Should().Be(ChargingState.ChargingPaused, "after some time we should be able to");
            _stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY_SMALL);
            _stateMachine.Unpause();
            _stateMachine.Current.Should().Be(ChargingState.ChargingPaused, "we can't unpause immediatly");
            _stateMachine.LastStateChange = DateTime.Now.AddSeconds(-DELAY);
            _stateMachine.Unpause();
            _stateMachine.Current.Should().Be(ChargingState.Charging, "after some time we should be able to");
        }
    }
}
