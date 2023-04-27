#if DEBUG

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;

namespace EMS.WebHost.Helpers;

public class AnalysisDiagnosticAdapter
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("AnalysisDiagnosticAdapter");

    [SuppressMessage("", "CA1822")]
    [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareStarting")]
    public void OnMiddlewareStarting(HttpContext httpContext, string name, Guid instance, long timestamp)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        Logger.Info("MiddlewareStarting: '{Name}'; Request Path: '{RequestPath}'", name, httpContext.Request.Path);
    }

    [SuppressMessage("", "CA1822")]
    [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareException")]
    public void OnMiddlewareException(Exception exception, HttpContext httpContext, string name, Guid instance, long timestamp, long duration)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(httpContext);
        Logger.Info("MiddlewareException: '{Name}'; '{Message}'", name, exception.Message);
    }

    [SuppressMessage("", "CA1822")]
    [DiagnosticName("Microsoft.AspNetCore.MiddlewareAnalysis.MiddlewareFinished")]
    public void OnMiddlewareFinished(HttpContext httpContext, string name, Guid instance, long timestamp, long duration)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        Logger.Info("MiddlewareFinished: '{Name}'; Status: '{StatusCode}'", name, httpContext.Response.StatusCode);
    }
}

#endif