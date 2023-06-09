using System;
using EMS.Library;
using Moq;
using Moq.Protected;
using P1SmartMeter.Connection;

namespace P1ReaderUnitTests
{
    public class P1ReaderTests
    {
        [Fact]
        public void DoesNotThrowExceptionWhenNoSubscribersToDataArrivedEvent()
        {
            var w = new Mock<IWatchdog>();
            using var tester = new P1ReaderTester(w.Object);
            Action a = () => tester.SomeData("qwerty");
            a.Should().NotThrow();
        }
        [Fact]
        public void ShouldRaiseEventWhenDataArrvives()
        {
            var w = new Mock<IWatchdog>();
            using var tester = new P1ReaderTester(w.Object);
            DataArrivedEventArgs? lastEvent = null;

            tester.DataArrived += (object? sender, DataArrivedEventArgs e) =>
            {
                lastEvent = e;
            };
            var arrivedData = "qwerty";
            tester.SomeData(arrivedData);
            lastEvent.Should().NotBeNull();
            ArgumentNullException.ThrowIfNull(lastEvent);   // get rid of warning
            lastEvent.Data.Should().BeEquivalentTo(arrivedData);
        }

        internal class P1ReaderTester : P1Reader
        {
            public P1ReaderTester(IWatchdog watchdog) : base(watchdog)
            {
            }

            public void SomeData(string data)
            {
                var dae = new DataArrivedEventArgs(data);
                this.OnDataArrived(dae);
            }

            protected override Task DoBackgroundWork()
            {
                throw new NotImplementedException();
            }

            protected override Task Start()
            {
                throw new NotImplementedException();
            }

            protected override void Stop()
            {
                throw new NotImplementedException();
            }
        }
    }
}
