using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using EMS.WebHost.Controllers;
using Newtonsoft.Json.Linq;

namespace EMS.WebHost.Integration.Tests;

public class UsersController
{
    private readonly string _dashes;
    private readonly Uri _baseAddress;

    public UsersController() {
        _dashes = "----------------------------";
        _baseAddress = new Uri("http://localhost:5005");
    }

    [Theory(DisplayName = "Ping - is method allowed")]
    [InlineData(HttpTools.Methods.POST, true)]
    [InlineData(HttpTools.Methods.PUT, true)]
    [InlineData(HttpTools.Methods.DELETE, true)]
    internal async void Test3(HttpTools.Methods method, bool methodNotAllowed)
    {
        Assert.Equal(methodNotAllowed, await HttpTools.MethodNotAllowed(_baseAddress, method, "api/users/ping").ConfigureAwait(true));
    }

    [Fact(DisplayName = "Ping - Should give 401 when not authenticated,")]
    public async void Test1()
    {
        using var _client = new HttpClient() { BaseAddress = _baseAddress };
        var response = await _client.GetAsync(new Uri(_client.BaseAddress, "api/users/ping")).ConfigureAwait(true);
# if DEBUG
        Console.WriteLine(_dashes);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(response.ToString());
#endif
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "Authenticate - Should give an error when credentials are invalid")]
    public async void Test5()
    {
        using var _client = new HttpClient() { BaseAddress = new Uri("http://localhost:5005") };
        var model = new LoginModel() { Username = "a", Password = "b" };
        using var content = JsonContent.Create<LoginModel>(model);
        var response = await _client.PostAsync(new Uri(_client.BaseAddress, "api/users/authenticate"), content).ConfigureAwait(true);
#if DEBUG
        Console.WriteLine(_dashes);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(response.ToString());
#endif
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "Authenticate - Should give a token when credentials are valid")]
    public async void Test6()
    {
        using var _client = new HttpClient() { BaseAddress = new Uri("http://localhost:5005") };
        var model = new LoginModel() { Username = "admin", Password = "admin" };
        using var content = JsonContent.Create<LoginModel>(model);
        var response = await _client.PostAsync(new Uri(_client.BaseAddress, "api/users/authenticate"), content).ConfigureAwait(true);
#if DEBUG
        Console.WriteLine(_dashes);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(response.ToString());
#endif
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).ConfigureAwait(true);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse?.User.Id.ToString()));
        Assert.False(string.IsNullOrWhiteSpace(loginResponse?.User.Name));
        Assert.False(string.IsNullOrWhiteSpace(loginResponse?.User.Username));
        Assert.False(string.IsNullOrWhiteSpace(loginResponse?.Token));
        Assert.Equal(200, loginResponse?.Status);
    }
}
