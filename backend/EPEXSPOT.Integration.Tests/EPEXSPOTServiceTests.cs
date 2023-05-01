using System;
namespace EPEXSPOT.Integration.Tests;

public class EPEXSPOTServiceTests
{
    [Fact]
    public async void IsAbleToRetrieveTheCurrentTariff()
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
    public async void IsAbleToRetrieveTariffForNext4Hours()
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
}