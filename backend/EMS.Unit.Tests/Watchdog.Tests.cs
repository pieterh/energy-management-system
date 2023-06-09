
using EMS.Library;
using EMS.Library.TestableDateTime;

namespace EMS.Unit.WatchdogTests;

public class WatchdogTest
{
	[Fact]
	public async Task InitialNoRestart()
	{
        using (new DateTimeProviderContext(new DateTime(2023, 6, 1, 12, 0, 0)))
        {
            using var watcher = new Watchdog();
            var worker = new Mock<IBackgroundWorker>();
            
            watcher.Register(worker.Object, 30);
            await watcher.PerformCheck().ConfigureAwait(false);

            worker.Verify(x => x.Restart(It.IsAny<bool>()), Times.Never);
        }
	}

    [Fact]
    public async Task RestartWhenDelay()
    {
        using (new DateTimeProviderContext(new DateTime(2023, 6, 1, 12, 0, 0)))
        {
            using var watcher = new Watchdog();
            var worker = new Mock<IBackgroundWorker>();
            
            watcher.Register(worker.Object, 30);
            using (new DateTimeProviderContext(new DateTime(2023, 6, 1, 12, 0, 40)))
            {
                await watcher.PerformCheck().ConfigureAwait(false);
            }

            worker.Verify(x => x.Restart(It.IsAny<bool>()), Times.Once);
        }
    }
}

