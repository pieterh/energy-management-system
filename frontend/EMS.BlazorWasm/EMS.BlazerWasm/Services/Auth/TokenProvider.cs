using System;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace EMS.BlazorWasm.Services.Auth
{
    public class TokenProvider : IAccessTokenProvider
    {
        private ILocalStorage _localStorage;
        public TokenProvider(ILocalStorage localStorage)
        {
            if (localStorage == null) throw new ArgumentNullException(nameof(localStorage));
            _localStorage = localStorage;
        }

        public async ValueTask<AccessTokenResult> RequestAccessToken()
        {
            var jwt = await _localStorage.GetStringAsync("token");
            var token = new AccessToken() { Value = jwt };
            var op = new InteractiveRequestOptions() { Interaction = InteractionType.SignIn, ReturnUrl = "login" };

            var accessTokenResult = new AccessTokenResult(AccessTokenResultStatus.Success, token, "login",op);
            return accessTokenResult;
        }

        public async ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
        {
            var jwt = await _localStorage.GetStringAsync("token");
            var token = new AccessToken() { Value = jwt };
            var op = new InteractiveRequestOptions() { Interaction = InteractionType.SignIn, ReturnUrl = "login" };

            var accessTokenResult = new AccessTokenResult(AccessTokenResultStatus.Success, token, "login", op);
            return accessTokenResult;

        }
    }
}

