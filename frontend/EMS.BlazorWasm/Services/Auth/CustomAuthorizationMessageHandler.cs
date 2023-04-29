using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace EMS.BlazorWasm.Services.Auth
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        private readonly IAccessTokenProvider _provider;
        private readonly NavigationManager _navigation;
        //private readonly IJSRuntime jsRuntime;

        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager)
        {
            _provider = provider;
            _navigation = navigationManager;
            ConfigureHandler(
               authorizedUrls: new[] { "http://localhost:5281/" });

        }

        //https://community.auth0.com/t/securing-blazor-webassembly-apps/46661/114
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri != null ? !request.RequestUri.AbsolutePath.EndsWith("authenticate", StringComparison.OrdinalIgnoreCase) : true)
            {
                var token = await _provider.RequestAccessToken();
                if (token.TryGetToken(out var t))
                {
                    var header = new AuthenticationHeaderValue("Bearer", t.Value);
                    request.Headers.Authorization = header;
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }


}

