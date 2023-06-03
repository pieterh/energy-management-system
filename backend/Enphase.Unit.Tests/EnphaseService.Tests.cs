using Moq;
using System.Diagnostics.CodeAnalysis;
using EMS.Library.Configuration;
using System.Net.Http.Headers;
using Enphase.DTO;
using Enphase.DTO.Info;
using EMS.Library;

namespace Enphase.Unit.Tests;

public class EnphaseServiceTests
{
    [Fact]
    [SuppressMessage("", "S1215")]
    public void DisposesProperly()
    {
        var mockFactory = new HttpClientFactoryMock();
        var w = new Mock<IWatchdog>();
        var mock = new Mock<EnphaseService>(new InstanceConfiguration() { EndPoint = "http://127.0.0.1" }, mockFactory, w.Object);
        mock.CallBase = true;

        mock.Object.Disposed.Should().BeFalse();

        mock.Object.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        mock.Object.Disposed.Should().BeTrue();
    }

    [Fact]
    [SuppressMessage("", "S1215")]
    public void DisposesCanSafelyCalledTwice()
    {
        var mockFactory = new HttpClientFactoryMock();
        var w = new Mock<IWatchdog>();
        var mock = new Mock<EnphaseService>(new InstanceConfiguration() { EndPoint = "http://127.0.0.1" }, mockFactory, w.Object);
        mock.CallBase = true;

        mock.Object.Disposed.Should().BeFalse();

        mock.Object.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        mock.Object.Disposed.Should().BeTrue();

        // and for the second time
        mock.Object.Dispose();
        mock.Object.Disposed.Should().BeTrue();
    }

    [Fact]
    [SuppressMessage("", "S1215")]
    public async Task CanStartStop()
    {
        var mockFactory = new HttpClientFactoryMock();
        var w = new Mock<IWatchdog>();
        var mock = new Mock<EnphaseService>(new InstanceConfiguration() { EndPoint = "http://127.0.0.1" }, mockFactory, w.Object);
        mock.CallBase = true;

        mock.Setup(x => x.GetData<InfoResponse>(It.Is<string>(m => m.Equals("/info.xml", StringComparison.Ordinal)), It.IsAny<MediaTypeWithQualityHeaderValue>(), It.IsAny<Func<HttpResponseMessage, CancellationToken, Task<InfoResponse>>>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.FromResult(
                                new InfoResponse()
                                {
                                    Device = new Device() { SerialNumber = "122011110962", Software = "R4.10.35", ApiVersion = "1", PartNumber = "800-00554-r03" },
                                    BuildInfo = new BuildInfo() { BuildId = "release-4.10.x-103-Nov-12-18-18:25:06", BuildTimeGmt= "1542157882" }
                                }
                            ));

        mock.Setup(x => x.GetData<ProductionStatusResponse>(It.Is<string>(m => m.Equals("/ivp/mod/603980032/mode/power", StringComparison.Ordinal)), It.IsAny<MediaTypeWithQualityHeaderValue>(), It.IsAny<Func<HttpResponseMessage, CancellationToken, Task<ProductionStatusResponse>>>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult<ProductionStatusResponse>(new ProductionStatusResponse()));

        mock.Setup(x => x.GetData<Inverter[]>(It.Is<string>(m => m.Equals("/api/v1/production/inverters", StringComparison.Ordinal)), It.IsAny<MediaTypeWithQualityHeaderValue>(), It.IsAny<Func<HttpResponseMessage, CancellationToken, Task<Inverter[]>>>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult<Inverter[]>(Array.Empty<Inverter>()));

        await mock.Object.StartAsync(CancellationToken.None).ConfigureAwait(false);
        mock.Object.BackgroundTask.Should().NotBeNull();
        await Task.Delay(250).ConfigureAwait(false);
        await mock.Object.StopAsync(CancellationToken.None).ConfigureAwait(false);
        mock.Object.Dispose();
        mock.Object.Disposed.Should().BeTrue();
    }

    internal sealed class HttpClientFactoryMock : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            throw new NotImplementedException();
        }
    }
}

