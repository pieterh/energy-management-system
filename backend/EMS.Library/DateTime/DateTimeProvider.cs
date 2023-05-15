// inspired by akazemis https://github.com/akazemis
namespace EMS.Library.TestableDateTime;

public static class DateTimeProvider
{
    public static System.DateTime Now
        => DateTimeProviderContext.Current == null
                ? System.DateTime.Now
                : DateTimeProviderContext.Current.ContextDateTimeNow.UtcDateTime;
}