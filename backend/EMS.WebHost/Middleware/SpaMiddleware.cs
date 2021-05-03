using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EMS.WebHost.Middleware
{
    public class SpaMiddleware
    {
        private RequestDelegate Next { get; init; }
        private ILogger Logger { get; init; }

        public SpaMiddleware(ILogger logger, RequestDelegate next)
        {
            Logger = logger;
            Next = next;
        }

        public Task Invoke(HttpContext context)
        {
            Logger.LogInformation($"{context.Request.Path}");
            var path = context.Request.Path;
            if (path.StartsWithSegments(new PathString("/app")) &&
                string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                context.Response.ContentType = "text/html";
                context.Request.Path = Path.Combine("/app", "index.html");
            }

            if (path.Equals(new PathString("/")))
            {
                context.Response.ContentType = "text/html";
                context.Request.Path = Path.Combine("/app", "index.html");
            }
            return Next(context);
        }    
    }
}
