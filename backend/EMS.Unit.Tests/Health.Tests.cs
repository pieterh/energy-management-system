using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EMS.Library.Shared.DTO;
using EMS.Library.Shared.DTO.Health;
using EMS.Library.TestableDateTime;

namespace EMS.Unit.Tests;

public class HealthTests
{
    [Fact]
    public async Task Healthy()
    {
        var m = new Mock<HealthCheck>(8080);
        m.Setup<Task<Response?>>((x) => x.RetrieveHealthStatus()).Returns(Task.FromResult<Response?>(new HealthResponse(DateTimeProvider.Now)));
        var hc = m.Object;

        var result = await hc.PerformHealthCheck().ConfigureAwait(false);
        result.Should().Be(0);
    }

    [Fact]
    public async Task Unhealthy1()
    {
        var m = new Mock<HealthCheck>(8080);
        m.Setup<Task<Response?>>((x) => x.RetrieveHealthStatus()).Returns(Task.FromResult<Response?>(new Response(500, "Oeps")));
        var hc = m.Object;

        var result = await hc.PerformHealthCheck().ConfigureAwait(false);
        result.Should().Be(1);
    }

    [Fact]
    public async Task Unhealthy2()
    {
        var m = new Mock<HealthCheck>(8080);
        m.Setup<Task<Response?>>((x) => x.RetrieveHealthStatus()).Returns(Task.FromResult<Response?>(null));
        var hc = m.Object;

        var result = await hc.PerformHealthCheck().ConfigureAwait(false);
        result.Should().Be(1);
    }

}
