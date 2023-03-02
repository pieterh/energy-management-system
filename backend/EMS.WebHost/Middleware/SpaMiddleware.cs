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

        private static readonly Action<ILogger, string, Exception> _invokeRequested = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(Invoke)),
                "SpaMiddleware Invoke --> {RequestPath}");

        public SpaMiddleware(ILogger logger, RequestDelegate next)
        {
            Logger = logger;
            Next = next;
        }

        public Task Invoke(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _invokeRequested(Logger, context.Request.Path, null);

            var path = context.Request.Path;

            // for api request, we don't do anything here and juts go to the next middleware
            if (path.StartsWithSegments(new PathString("/api"), StringComparison.Ordinal))
                return Next(context);

            // request for app files
            if (path.StartsWithSegments(new PathString("/app"), StringComparison.Ordinal)){
                // loading the main page? then go to the index
                if (string.IsNullOrEmpty(Path.GetExtension(path)))
                {
                    context.Response.ContentType = "text/html";
                    context.Request.Path = Path.Combine("/app", "index.html");
                }
                return Next(context);
            }
            
            return Next(context);
        }    
    }
}
