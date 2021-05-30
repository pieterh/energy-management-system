using System;

// inspired by akazemis https://github.com/akazemis
namespace EMS.Library.TestableDateTime
{
    public static class DateTimeProvider
    {
        public static DateTime Now
            => DateTimeProviderContext.Current == null
                    ? DateTime.Now
                    : DateTimeProviderContext.Current.ContextDateTimeNow;
    }
}
