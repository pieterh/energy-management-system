using System;
using System.Collections;
using System.Threading;

// inspired by akazemis https://github.com/akazemis
namespace EMS.Library.TestableDateTime;

public class DateTimeProviderContext : IDisposable
{
    private bool _disposed;

    internal System.DateTimeOffset ContextDateTimeNow;
    private static readonly ThreadLocal<Stack> ThreadScopeStack = new(() => new Stack());

    public DateTimeProviderContext(System.DateTimeOffset contextDateTimeOffsetNow)
    {
        ContextDateTimeNow = contextDateTimeOffsetNow;
        if (!ThreadScopeStack.IsValueCreated)
            ThreadScopeStack.Value = new Stack();

        ThreadScopeStack.Value?.Push(this);
    }

    public DateTimeProviderContext(System.DateTime contextDateTimeNow)
        : this(new DateTimeOffset(contextDateTimeNow))
    {        
    }

    public static DateTimeProviderContext? Current
    {
        get
        {
            if (ThreadScopeStack.Value?.Count == 0)
                return null;
            else
                return ThreadScopeStack.Value?.Peek() as DateTimeProviderContext;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);  // Suppress finalization.
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            ThreadScopeStack.Value?.Pop();
        }

        _disposed = true;
    }
}