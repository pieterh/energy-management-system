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
            Logger.LogInformation("SpaMiddleware Invoke -> {requestPath}", context.Request.Path);
            var path = context.Request.Path;

            // for api request, we don't do anything here and juts go to the next middleware
            if (path.StartsWithSegments(new PathString("/api")))
                return Next(context);

            // request for app files
            if (path.StartsWithSegments(new PathString("/app"))){
                // loading the main page? then go to the index
                if (string.IsNullOrEmpty(Path.GetExtension(path)))
                {
                    context.Response.ContentType = "text/html";
                    context.Request.Path = Path.Combine("/app", "index.html");
                }
                return Next(context);
            }
            
            // it looks like the page is reloading, just serve the index page
            //context.Response.ContentType = "text/html";
            //context.Request.Path = Path.Combine("/app", "index.html");           
            
            return Next(context);
        }    
    }
}
