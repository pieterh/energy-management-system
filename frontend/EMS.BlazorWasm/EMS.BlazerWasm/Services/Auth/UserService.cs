using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;

using EMS.BlazorWasm.Services;

namespace EMS.BlazorWasm.Client.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationProvider _authenticationStateProvider;
        private readonly EMS.BlazorWasm.Services.ILocalStorage _localStorage;

        public UserService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILocalStorage localStorage)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            _httpClient = httpClient;

            if (authenticationStateProvider == null || !(authenticationStateProvider is CustomAuthenticationProvider))
                throw new ArgumentOutOfRangeException(nameof(authenticationStateProvider));
            _authenticationStateProvider = (CustomAuthenticationProvider)authenticationStateProvider;

            _localStorage = localStorage;
        }

        public async Task<LoginResponse> LoginAsync(LoginModel model)
        {
            
            var jsonString = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var r = await _httpClient.PostAsync("api/users/authenticate", content);
            if (r == null) throw new NullReferenceException("Oeps");
            string responseBody = await r.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (loginResponse == null) throw new NullReferenceException("Oeps2");

            var (user, token) = loginResponse;
            if (user == null) throw new NullReferenceException("Oeps3");

            await _localStorage.SaveObjectAsync("user", user);
            await _localStorage.SaveStringAsync("token", token);
            _authenticationStateProvider.LoginNotify();
            return loginResponse;
        }

        public async void LogoutAsync()
        {
            await _localStorage.RemoveAsync("user");
            await _localStorage.RemoveAsync("token");
            _authenticationStateProvider.LogoutNotify();
        }

        public async Task<PingResponse> Ping()
        {
            var r = await _httpClient.GetFromJsonAsync<PingResponse>("api/users/ping");
            if (r == null) throw new NullReferenceException("Oeps");
            return r;
        }
    }    
}