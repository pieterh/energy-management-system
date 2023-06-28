using EMS.Library.TestableDateTime;

namespace Enphase.Unit.Tests;

public class AuthenticationUnitTests
{
    [Fact]
    void GetMobilePasswdForSerialBasic()
    {
        var str = Authentication.GetMobilePasswdForSerial("122011110123", "installer");
        str.Should().Be("d5ceb265");
    }

    [Fact]
    void GetMobilePasswdForSerialTakesUsername()
    {
        var str = Authentication.GetMobilePasswdForSerial("122011110123", "someone");
        str.Should().Be("d2eg979a");
    }

    [Fact]
    void GetMobilePasswdForSerialTakesRealm()
    {
        string str;
        str = Authentication.GetMobilePasswdForSerial("122011110123", "someone", "hems");
        str.Should().Be("b83f97Ac");

        str = Authentication.GetMobilePasswdForSerial("122011110123", "someone", string.Empty);
        str.Should().Be("d2eg979a", "empty is the default");
    }

    [Fact]
    void GetMobilePasswdForSerialTakesNullsAndEmpty()
    {
        string pwd;
        pwd = Authentication.GetMobilePasswdForSerial(string.Empty, "someone", "hems");
        pwd.Should().BeEmpty("serialNumber is empty");

        pwd = Authentication.GetMobilePasswdForSerial("122011110123", string.Empty, "hems");
        pwd.Should().BeEmpty("userName is empty");
    }

    [Fact]
    void GetPublicPasswdBasic()
    {
        string pwd;
        // different datetimeoffsets, should create different passwords
        using (new DateTimeProviderContext(new DateTimeOffset(2023, 6, 27, 12, 0, 0, new TimeSpan(2, 0, 0))))
        {
            pwd = Authentication.GetPublicPasswd("122011110123", "installer");
            pwd.Should().Be("6ecb2b39696bf704933f93e7d4bb8357");
        }

        // so a day later we expect a different value
        using (new DateTimeProviderContext(new DateTimeOffset(2023, 6, 28, 12, 0, 0, new TimeSpan(2, 0, 0))))
        {
            pwd = Authentication.GetPublicPasswd("122011110123", "installer");
            pwd.Should().Be("e84bc0a992bef212dcc75d9a026c851b");
        }
    }
}