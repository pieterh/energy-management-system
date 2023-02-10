using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;

using EMS.BlazorWasm.Services;

namespace EMS.BlazorWasm.Client.Services.Auth
{
    public class CustomAuthenticationProvider : AuthenticationStateProvider
    {
        private static readonly ClaimsPrincipal _anonymousPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        private readonly ILocalStorage _localStorage;
        private ClaimsPrincipal? _claimsPrincipal = null;

        public CustomAuthenticationProvider(ILocalStorage localStorage){
            _localStorage = localStorage;            
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await Task.FromResult(0);
            /* get the initial claims from token (if it is already present) */
            if (_claimsPrincipal == null)
                _claimsPrincipal = await GetClaimsFromToken();

            return new AuthenticationState(_claimsPrincipal);
        }

        public async void LoginNotify()
        {
            _claimsPrincipal = await GetClaimsFromToken();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void LogoutNotify()
        {            
            _claimsPrincipal = _anonymousPrincipal;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private async Task<ClaimsPrincipal> GetClaimsFromToken()
        {
            var token = await _localStorage.GetStringAsync("token");
            if (string.IsNullOrWhiteSpace(token))
                return _anonymousPrincipal;

            var claims = JwtParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "JwtBearer");
            return new ClaimsPrincipal(identity);
        }
    }

    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payload);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
                claims.AddRange(keyValuePairs
                    .Where(kvp => kvp.Value != null)
                    .Select((kvp) => new Claim(kvp.Key, kvp.Value.ToString()!) // keep compiler happy, since null values are already filtered out in previous step
                ));

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}