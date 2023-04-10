using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace EMS.WebHost.Integration.Tests
{
	public static class HttpClientExtensions
	{
        public static Task<HttpResponseMessage> OptionsAsync(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri) =>
            OptionsAsync(client, CreateUri(requestUri));
        public static Task<HttpResponseMessage> OptionsAsync(this HttpClient client, Uri? requestUri) =>
            OptionsAsync(client, requestUri, CancellationToken.None);
        public static Task<HttpResponseMessage> OptionsAsync(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken) =>
            OptionsAsync(client, CreateUri(requestUri), cancellationToken);

        [SuppressMessage("", "CA2000")]
        [SuppressMessage("", "CA1062")]
        public static Task<HttpResponseMessage> OptionsAsync(this HttpClient client, Uri? requestUri, CancellationToken cancellationToken)
        {            
            HttpRequestMessage request = CreateRequestMessage(HttpMethod.Options, requestUri);
            return client.SendAsync(request, cancellationToken);
        }

        #region Private Helpers
        private static Uri? CreateUri(string? uri) =>
            string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
        private static HttpRequestMessage CreateRequestMessage(HttpMethod method, Uri? uri) =>
            new HttpRequestMessage(method, uri) { };
        #endregion Private Helpers
    }
}

