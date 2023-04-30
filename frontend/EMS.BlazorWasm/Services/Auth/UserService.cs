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
using EMS.Library.Shared.DTO;
using EMS.Library.Shared.DTO.Users;

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

            if (!(authenticationStateProvider is CustomAuthenticationProvider))
                throw new ArgumentOutOfRangeException(nameof(authenticationStateProvider));
            _authenticationStateProvider = (CustomAuthenticationProvider)authenticationStateProvider;

            if (localStorage == null) throw new ArgumentNullException(nameof(localStorage));
            _localStorage = localStorage;
        }

        public async Task<Response?> LoginAsync(LoginModel model)
        {
            return await LoginAsync(model, CancellationToken.None);
        }

        public async Task<Response?> LoginAsync(LoginModel model, CancellationToken cancellationToken)
        {
            var content = JsonContent.Create<LoginModel>(model);
            var r = await _httpClient.PostAsync("api/users/authenticate", content, cancellationToken);
            if (r == null) throw new InvalidOperationException("Oeps");
            var loginResponse = await r.Content.ReadFromJsonAsync<Response>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            if (loginResponse == null) throw new InvalidOperationException("Oeps3");

            if (loginResponse.Status == 200)
            {
                var (user, token) = (LoginResponse)loginResponse;
                if (user == null) throw new InvalidOperationException("Oeps3");

                await _localStorage.SaveObjectAsync("user", user);
                await _localStorage.SaveStringAsync("token", token);
                _authenticationStateProvider.LoginNotify();
            }
            return loginResponse;
        }

        public async void LogoutAsync()
        {
            await _localStorage.RemoveAsync("user");
            await _localStorage.RemoveAsync("token");
            _authenticationStateProvider.LogoutNotify();
        }

        public async Task<SetPasswordResponse> SetPasswordAsync(SetPasswordModel model, CancellationToken cancellationToken)        
        {
            var content = JsonContent.Create<SetPasswordModel>(model);
            var r = await _httpClient.PostAsync("api/users/setpassword", content, cancellationToken);
            if (r == null) throw new InvalidOperationException("Oeps");
            var setPasswordResponse = await r.Content.ReadFromJsonAsync<SetPasswordResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            if (setPasswordResponse == null) throw new InvalidOperationException("Oeps2");
            return setPasswordResponse;
        }

        public async Task<PingResponse> Ping()
        {
            var r = await _httpClient.GetFromJsonAsync<PingResponse>("api/users/ping");
            if (r == null) throw new InvalidOperationException("Oeps");
            return r;
        }
    }    
}