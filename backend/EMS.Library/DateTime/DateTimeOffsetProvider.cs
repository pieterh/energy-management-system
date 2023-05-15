

namespace EMS.Library.TestableDateTime;

public static class DateTimeOffsetProvider
{
    public static System.DateTimeOffset Now
        => DateTimeProviderContext.Current == null
                ? System.DateTimeOffset.Now
                : DateTimeProviderContext.Current.ContextDateTimeNow;
}

