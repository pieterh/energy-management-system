using System;
using EMS.WebHost.Controllers;
using System.Net.Http.Json;
using System.Diagnostics.CodeAnalysis;

namespace EMS.WebHost.Integration.Tests
{
    public enum Methods { POST, PUT, DELETE, GET }
    internal static class HttpTools
    {
#if DEBUG

        private const string _dashes = "----------------------------";
        [SuppressMessage("", "CA1303")]
#endif

        public static async Task<bool> MethodNotAllowed(Uri baseUri, string relativeUrl, Methods method, string path)
        {
            HttpResponseMessage response;
            using var _client = new HttpClient() { BaseAddress = baseUri };
            switch (method)
            {
                case Methods.POST:
                    {
                        var model = new LoginModel() { Username = "a", Password = "b" };
                        using var content = JsonContent.Create<LoginModel>(model);
                        response = await _client.PostAsync(new Uri(_client.BaseAddress, relativeUrl), content).ConfigureAwait(true);
                    }
                    break;
                case Methods.PUT:
                    {
                        var model = new LoginModel() { Username = "a", Password = "b" };
                        using var content = JsonContent.Create<LoginModel>(model);
                        response = await _client.PutAsync(new Uri(_client.BaseAddress, relativeUrl), content).ConfigureAwait(true);
                    }
                    break;
                case Methods.DELETE:
                    {
                        response = await _client.DeleteAsync(new Uri(_client.BaseAddress, relativeUrl)).ConfigureAwait(true);
                    }
                    break;
                case Methods.GET:
                    {
                        response = await _client.GetAsync(new Uri(_client.BaseAddress, relativeUrl)).ConfigureAwait(true);
                    }
                    break;
                default:
                    return true;
            }

#if DEBUG
            Console.WriteLine(_dashes);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.ToString());
#endif
            return response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed;
        }


    }
}

