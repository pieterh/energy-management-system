
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Adapter.Solar;
using EMS.Library.TestableDateTime;

namespace EMS.Unit.Tests;

public class SolarOptimizerTests
{
    [Fact]
    public void CreateAndDispose()
    {
        var p = new Mock<IPriceProvider>();
        var s = new Mock<ISolar>();
        using (new DateTimeProviderContext(new DateTime(2023, 05, 22, 12, 50, 0)))
        {
            var optimizer = new Mock<SolarOptimizer>(p.Object, s.Object);
            var mObj = optimizer.Object;
            mObj.Disposed.Should().BeFalse();
            mObj.Dispose();
            mObj.Disposed.Should().BeTrue();
        }
    }

    [Fact]
    [SuppressMessage("", "S3966")]
    public void CreateAndDoubleDispose()
    {
        var p = new Mock<IPriceProvider>();
        var s = new Mock<ISolar>();
        using (new DateTimeProviderContext(new DateTime(2023, 05, 22, 12, 50, 0)))
        {
            var optimizer = new Mock<SolarOptimizer>(p.Object, s.Object);
            var mObj = optimizer.Object;
            mObj.Disposed.Should().BeFalse();
            mObj.Dispose();
            mObj.Disposed.Should().BeTrue();
            mObj.Dispose();
            mObj.Disposed.Should().BeTrue();
        }
    }

    [Fact]
    public void CanStartStop()
    {
        var p = new Mock<IPriceProvider>();
        var s = new Mock<ISolar>();

        // don't async / await with the datetimeprovider
        using (new DateTimeProviderContext(new DateTime(2023, 05, 22, 12, 50, 0)))
        {
            var optimizer = new Mock<SolarOptimizer>(p.Object, s.Object);
            optimizer.CallBase = true;
            var mObj = optimizer.Object;

            // max duration of test
            using var timeout = new CancellationTokenSource(new TimeSpan(0, 0, 30));

            mObj.StartAsync(timeout.Token).GetAwaiter().GetResult();
            Task.Delay(500).GetAwaiter().GetResult(); // let it spin a bit
            mObj.StopAsync(timeout.Token).GetAwaiter().GetResult();

            mObj.Dispose();
            mObj.Disposed.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CanPerformCheck()
    {
        var p = new Mock<IPriceProvider>();
        var s = new Mock<ISolar>();
        using (new DateTimeProviderContext(new DateTime(2023, 05, 22, 12, 50, 0)))
        {
            var optimizer = new Mock<SolarOptimizer>(p.Object, s.Object);
            await optimizer.Object.PerformCheck().ConfigureAwait(false);
        }
    }
}
