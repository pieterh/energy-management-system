using BackgroundWorker;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace EMS.Library.Unit.Tests
{
    public class BackgroundServiceTests
    {
        [Fact(DisplayName = "Dispose safely")]
        void CanDisposeOnce()
        {
            var mock = new Mock<BackgroundService>();
            mock.CallBase = true;
            Assert.False(mock.Object.Disposed);
            mock.Object.Dispose();
            Assert.True(mock.Object.Disposed);
        }

        [Fact(DisplayName = "Dispose safely twice")]
        void CanDisposeTwice()
        {
            var mock = new Mock<BackgroundService>();
            mock.CallBase = true;
            Assert.False(mock.Object.Disposed);
            mock.Object.Dispose();
            Assert.True(mock.Object.Disposed);
            mock.Object.Dispose();
            Assert.True(mock.Object.Disposed);
        }

        [Fact(DisplayName = "Stop requested waits when not canceled")]
        async Task HandleNoStoprequested()
        {
            var mock = new Mock<BackgroundService>();
            var start = DateTime.Now;
            Assert.False(await mock.Object.StopRequested(500).ConfigureAwait(false));
            // should delay a bit when not canceled
            var duration = DateTime.Now - start;
            // lower range a bit lower then the 500, since in some cases it is quicker to finish
            duration.Milliseconds.Should().BeInRange(475, 1000, because: "It should wait atleast a bit");

        }
        [Fact(DisplayName = "Stop requested handles cancel")]
        async Task HandleStopRequested()
        {
            var mock = new Mock<BackgroundService>();
            var start =  DateTime.Now;
            mock.Object.TokenSource.CancelAfter(250);
            await Task.Run(async () =>
            {
                Assert.True(await mock.Object.StopRequested(7000).ConfigureAwait(false));
            }).ConfigureAwait(false);

            // should be canceled within a reasonable time
            var duration =  DateTime.Now - start;
            duration.TotalMilliseconds.Should().BeLessThanOrEqualTo(750);
        }
        [Fact(DisplayName = "StopRequested accepts no waiting")]
        async Task AcceptsNoDelayForStopping()
        {
            var mock = new Mock<BackgroundService>();
            Assert.False(await mock.Object.StopRequested(0).ConfigureAwait(false));
        }
        [Fact(DisplayName = "StopRequested can be called if already stopped")]
        async Task AlreadyStoppped()
        {
            var mock = new Mock<BackgroundService>();
            mock.Object.TokenSource.Cancel();
            Assert.True(await mock.Object.StopRequested(0).ConfigureAwait(false));
        }
    }
}
