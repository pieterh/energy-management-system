using System;
namespace EPEXSPOT.Integration.Tests;

public class EPEXSPOTServiceTests
{
    [Fact]
    public async Task IsAbleToRetrieveTheCurrentTariff()
    {
        using var client = new HttpClient();
        var getapxtariffsUri = new Uri(new Uri("https://mijn.easyenergy.com"), EPEXSPOTService.getapxtariffsMethod);
        var now = DateTime.Now.ToUniversalTime();
        var t = await EPEXSPOTService.GetTariff(client, getapxtariffsUri, now, now.AddHours(1)).ConfigureAwait(false);
        t.Should().NotBeNull();
        t.Should().HaveCount(1);
        t[0].Timestamp.Should().BeCloseTo(now, new TimeSpan(1, 0, 0));
        t[0].TariffUsage.Should().BeInRange(-1, 1);
        t[0].TariffReturn.Should().BeInRange(-1, 1);
    }

    [Fact]
    public async Task IsAbleToRetrieveTariffForNext4Hours()
    {
        using var client = new HttpClient();
        var getapxtariffsUri = new Uri(new Uri("https://mijn.easyenergy.com"), EPEXSPOTService.getapxtariffsMethod);
        var now = DateTime.Now.ToUniversalTime();
        var t = await EPEXSPOTService.GetTariff(client, getapxtariffsUri, now, now.AddHours(4)).ConfigureAwait(false);
        t.Should().NotBeNull();
        t.Should().HaveCount(4);
        t[0].Timestamp.Should().BeCloseTo(now, new TimeSpan(1, 0, 0));
        t[0].TariffUsage.Should().BeInRange(-1, 1);
        t[0].TariffReturn.Should().BeInRange(-1, 1);

        t[3].Timestamp.Should().BeCloseTo(now.AddHours(3), new TimeSpan(1, 0, 0));
    }

    [Fact]
    public async Task IsAbleToRetrieveTariffForNext4HoursNegative1()
    {
        using var client = new HttpClient();
        var getapxtariffsUri = new Uri(new Uri("https://mijn.easyenergy.com"), EPEXSPOTService.getapxtariffsMethod);
        var now = new DateTime(2023, 5, 13, 6, 00, 00, DateTimeKind.Utc);
        var t = await EPEXSPOTService.GetTariff(client, getapxtariffsUri, now, now.AddHours(16)).ConfigureAwait(false);
        t.Should().NotBeNull();
        t.Should().HaveCount(16);
        t[0].Timestamp.Should().BeCloseTo(now, new TimeSpan(1, 0, 0));
        t[0].TariffUsage.Should().BePositive();
        t[0].TariffReturn.Should().BePositive();

        t[4].Timestamp.Should().BeCloseTo(now.AddHours(4), new TimeSpan(1, 0, 0));
        t[4].TariffReturn.Should().BeNegative();
    }

    [Fact]
    public async Task IsAbleToRetrieveTariffForNext4HoursNegative2()
    {
        using var client = new HttpClient();
        var getapxtariffsUri = new Uri(new Uri("https://mijn.easyenergy.com"), EPEXSPOTService.getapxtariffsMethod);
        var now = new DateTime(2023, 5, 20, 8, 00, 00, DateTimeKind.Utc);
        var t = await EPEXSPOTService.GetTariff(client, getapxtariffsUri, now, now.AddHours(8)).ConfigureAwait(false);
        t.Should().NotBeNull();
        t.Should().HaveCount(8);
        t[0].Timestamp.Should().BeCloseTo(now, new TimeSpan(1, 0, 0));
        t[0].TariffUsage.Should().BePositive();
        t[0].TariffReturn.Should().BePositive();

        t[4].Timestamp.Should().BeCloseTo(now.AddHours(4), new TimeSpan(1, 0, 0));
        t[4].TariffReturn.Should().BeNegative();
    }
}