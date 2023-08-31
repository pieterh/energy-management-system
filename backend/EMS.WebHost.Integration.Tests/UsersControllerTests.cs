using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using EMS.Library.Shared.DTO.Users;
using EMS.WebHost.Controllers;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace EMS.WebHost.Integration.Tests
{
    public class UsersControllerTests
    {
        const string BASE_ADDRESS = "http://localhost:5005";

#if DEBUG
        private readonly string _dashes;
#endif
        private readonly Uri _baseAddress;

        public UsersControllerTests()
        {
#if DEBUG
            _dashes = "----------------------------";
#endif
            _baseAddress = new Uri(BASE_ADDRESS);
        }

        [Theory(DisplayName = "Ping - method not allowed")]
        [InlineData(Methods.POST, true)]
        [InlineData(Methods.PUT, true)]
        [InlineData(Methods.DELETE, true)]
        public async Task Test1(Methods method, bool methodNotAllowed)
        {
            Assert.Equal(methodNotAllowed, await HttpTools.MethodNotAllowed(_baseAddress, "api/users/ping", method, "api/users/ping").ConfigureAwait(true));
        }

        [Fact(DisplayName = "Ping - Should give 401 when not authenticated,")]
        public async Task Test2()
        {
            using var _client = new HttpClient() { BaseAddress = _baseAddress };
            var response = await _client.GetAsync(new Uri(_client.BaseAddress, "api/users/ping")).ConfigureAwait(true);
#if DEBUG
            Console.WriteLine(_dashes);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.ToString());
#endif
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact(DisplayName = "Authenticate - Should give an error when credentials are invalid")]
        public async Task Test5()
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
        public async Task Test6()
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
            Assert.NotNull(loginResponse);
            Assert.False(string.IsNullOrWhiteSpace(loginResponse.User.Id.ToString()));
            Assert.False(string.IsNullOrWhiteSpace(loginResponse.User.Name));
            Assert.False(string.IsNullOrWhiteSpace(loginResponse.User.Username));
            Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token));
            Assert.Equal(200, loginResponse.Status);
        }
    }

    public class BlazorWasm
    {
        const string BASE_ADDRESS = "http://localhost:5005";
        private readonly Uri _baseAddress;

        public BlazorWasm()
        {
            _baseAddress = new Uri(BASE_ADDRESS);
        }

        [Fact(DisplayName = "BlazorWasm - should return the title page as the home page")]
        public async Task Test1()
        {
            using var _client = new HttpClient() { BaseAddress = _baseAddress };
            using var response = await _client.GetAsync(new Uri(_client.BaseAddress, "")).ConfigureAwait(true);
            
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var t = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            Assert.Equal(t, response.Content.Headers.ContentType);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            content.Contains("<title>EMS.BlazorWasm</title>", StringComparison.OrdinalIgnoreCase);
            content.Should().Contain("<title>EMS.BlazorWasm</title>");
        }

        [Fact(DisplayName = "BlazorWasm - should be able to load the referenced files")]
        public async Task Test2()
        {
            using var _client = new HttpClient() { BaseAddress = _baseAddress };
            using var response = await _client.GetAsync(new Uri(_client.BaseAddress, "")).ConfigureAwait(true);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var t = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            Assert.Equal(t, response.Content.Headers.ContentType);
            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            HtmlDocument htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(html);

            foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//link[@href]|//script[@src]"))
            {
                HtmlAttribute attHref = link.Attributes["href"];
                HtmlAttribute attSrc = link.Attributes["src"];
                string? url = attHref?.Value ?? attSrc?.Value;

                if (url != null && !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
#if DEBUG
                    Console.WriteLine($"Validating -> {url} ");
#endif
                    using var response2 = await _client.GetAsync(new Uri(_client.BaseAddress, url)).ConfigureAwait(true);
                    
                    response2.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "we expect to be able to download file {0}", url);
                    _ = await response2.Content.ReadAsStringAsync().ConfigureAwait(true);
                }
#if DEBUG
                else
                {
                    Console.WriteLine($"Not validating -> {url} ");
                }
#endif
            }

        }
    }
}