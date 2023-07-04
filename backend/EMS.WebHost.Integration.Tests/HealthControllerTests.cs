using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;

using EMS.Library.Shared.DTO;
using EMS.Library.Shared.DTO.Health;
using EMS.Library.Shared.DTO.Users;

namespace EMS.WebHost.Integration.Tests;

public class HealthControllerTests
{

    const string BASE_ADDRESS = "http://localhost:5005";
    private readonly Uri _baseAddress;
    private static TimeSpan Timeout = new TimeSpan(0, 0, 15);

    public HealthControllerTests()
    {
        _baseAddress = new Uri(BASE_ADDRESS);
    }

    [Theory(DisplayName = "Check - method not allowed")]
    [InlineData(Methods.POST, true)]
    [InlineData(Methods.PUT, true)]
    [InlineData(Methods.DELETE, true)]
    public async Task Test1(Methods method, bool methodNotAllowed)
    {
        var result = await HttpTools.MethodNotAllowed(_baseAddress, "api/health/check", method, "api/health/check").ConfigureAwait(true);
        result.Should().Be(methodNotAllowed);
    }

    [Fact(DisplayName = "Check - Should give an healthy status")]
    public async Task Test5()
    {
        using var _client = new HttpClient() { BaseAddress = _baseAddress };

        using var cancellationTokenSource = new CancellationTokenSource(Timeout);

        var response = await _client.GetAsync(new Uri(_client.BaseAddress, "api/health/check"), cancellationTokenSource.Token).ConfigureAwait(true);
#if DEBUG
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(response.ToString());
#endif
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var c = await response.Content.ReadFromJsonAsync<Response>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationTokenSource.Token).ConfigureAwait(false);
        c.Should().NotBeNull();
        c?.Should().BeAssignableTo<Response>();
        c?.Status.Should().Be(200);
        c?.Should().BeAssignableTo<HealthResponse>();
    }
}