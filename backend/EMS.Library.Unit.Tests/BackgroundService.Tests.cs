using BackgroundWorker;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
        void HandleNoStoprequested()
        {
            var mock = new Mock<BackgroundService>();
            var start = DateTime.Now;
            Assert.False(mock.Object.StopRequested(500));
            // should delay a bit when not canceled
            var duration = DateTime.Now - start;
            Assert.True(duration.TotalMilliseconds > 500);
        }
        [Fact(DisplayName = "Stop requested handles cancel")]
        async void HandleStopRequested()
        {
            var mock = new Mock<BackgroundService>();
            var start =  DateTime.Now;
            mock.Object.TokenSource.CancelAfter(100);
            await Task.Run(() =>
            {
                Assert.True(mock.Object.StopRequested(5000));
            }).ConfigureAwait(false);

            // should be canceled within a reasonable time
            var duration =  DateTime.Now - start;
            Assert.True(duration.TotalMilliseconds < 500);
        }
        [Fact(DisplayName = "StopRequested accepts no waiting")]
        void AcceptsNoDelayForStopping()
        {
            var mock = new Mock<BackgroundService>();
            Assert.False(mock.Object.StopRequested(0));
        }
        [Fact(DisplayName = "StopRequested can be called if already stopped")]
        void AlreadyStoppped()
        {
            var mock = new Mock<BackgroundService>();
            mock.Object.TokenSource.Cancel();
            Assert.True(mock.Object.StopRequested(0));
        }
    }
}
