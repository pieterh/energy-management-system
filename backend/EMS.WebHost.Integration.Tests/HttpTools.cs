﻿using System;
using EMS.WebHost.Controllers;
using System.Net.Http.Json;

namespace EMS.WebHost.Integration.Tests
{
	internal static class HttpTools
	{
        public enum Methods {POST, PUT, DELETE }
        private const string _dashes = "----------------------------";

        public static async Task<bool> MethodNotAllowed(Uri baseUri, Methods method, string path)
        {
            HttpResponseMessage response;
            using var _client = new HttpClient() { BaseAddress = new Uri("http://localhost:5005") };
            switch (method)
            {
                case Methods.POST:
                    {
                        var model = new LoginModel() { Username = "a", Password = "b" };
                        using var content = JsonContent.Create<LoginModel>(model);
                        response = await _client.PostAsync(new Uri(_client.BaseAddress, "api/users/ping"), content).ConfigureAwait(true);
                    }
                    break;
                case Methods.PUT:
                    {
                        var model = new LoginModel() { Username = "a", Password = "b" };
                        using var content = JsonContent.Create<LoginModel>(model);
                        response = await _client.PutAsync(new Uri(_client.BaseAddress, "api/users/ping"), content).ConfigureAwait(true);
                    }
                    break;
                case Methods.DELETE:
                    {
                        response = await _client.DeleteAsync(new Uri(_client.BaseAddress, "api/users/ping")).ConfigureAwait(true);
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
