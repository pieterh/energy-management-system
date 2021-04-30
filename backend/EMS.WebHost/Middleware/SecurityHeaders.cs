using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EMS.WebHost.Middleware
{
    public sealed class SecurityHeaders
    {       
        public RequestDelegate Next  { get; init;}

        public SecurityHeaders(RequestDelegate next)
        {
            Next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "no-referrer");
            //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; report-uri /idgreport");
            context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
            return Next(context);
        }
    }
}
