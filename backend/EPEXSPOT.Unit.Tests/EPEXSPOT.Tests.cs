using Xunit;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.TestableDateTime;

namespace EPEXSPOT.Tests;

public class TariffTests
{
    [Theory]
    [MemberData(nameof(TestDataGenerator.Correct), MemberType = typeof(TestDataGenerator))]
    public void FindsCorrectTariff(Tariff[] tariff, DateTime dateTime, int idx)
    {
        ArgumentNullException.ThrowIfNull(tariff);
        Tariff? t = EPEXSPOTService.FindTariff(tariff, dateTime);
        Assert.NotNull(t);
        Assert.Equal(tariff[idx].Timestamp, t?.Timestamp);
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.NotAvailable), MemberType = typeof(TestDataGenerator))]
    public void TariffNotAvailable(Tariff[] tariff, DateTime dateTime)
    {
        var t = EPEXSPOTService.FindTariff(tariff, dateTime);
        Assert.Null(t);
    }

    [Fact]
    public void HandlesEmpty()
    {
        var t = EPEXSPOTService.FindTariff(Array.Empty<Tariff>(), new DateTime(2021, 12, 29, 12, 00, 00, DateTimeKind.Utc));
        Assert.Null(t);
    }

    [Fact]
    public void ValidatesArguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var t = EPEXSPOTService.FindTariff(
                        new Tariff[] { new Tariff(new DateTime(2021, 12, 29, 13, 00, 00), 0, 0) },
                        new DateTime(2021, 12, 29, 12, 00, 00, DateTimeKind.Unspecified));
            t.Should().NotBeNull();
        });
    }
}

public static class TestDataGenerator
{
    public static IEnumerable<object[]> NotAvailable()
    {
        yield return new object[]
        {
                new Tariff[] { new Tariff(new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc), 0, 0) },
                new DateTime(2021, 12, 29, 12, 00, 00, DateTimeKind.Utc)
        };

        yield return new object[]
         {
                new Tariff[] { new Tariff(new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc), 0, 0),
                               new Tariff(new DateTime(2021, 12, 29, 14, 00, 00, DateTimeKind.Utc), 0, 0) },
                new DateTime(2021, 12, 29, 14, 05, 00, DateTimeKind.Utc)
         };

        yield return new object[]
        {
                new Tariff[] {new Tariff(new DateTime(2021, 12, 29, 13, 30, 00, DateTimeKind.Utc), 0, 0) } ,
                new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc)
        };
    }

    public static IEnumerable<object[]> Correct()
    {
        yield return new object[]
        {
                new Tariff[] {new Tariff(new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc), 0, 0) },
                new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc),
                0
        };
        yield return new object[]
        {
                new Tariff[] {
                    new Tariff(new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc), 0, 0),
                    new Tariff(new DateTime(2021, 12, 29, 14, 00, 00, DateTimeKind.Utc), 0, 0)},
                new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc),
                0
        };
        yield return new object[]
        {
                new Tariff[] {
                    new Tariff(new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc), 0, 0),
                    new Tariff(new DateTime(2021, 12, 29, 14, 00, 00, DateTimeKind.Utc), 0, 0)},
                new DateTime(2021, 12, 29, 13, 30, 00, DateTimeKind.Utc),
                0
        };
        yield return new object[]
        {
                new Tariff[] {
                    new Tariff(new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc), 0, 0),
                    new Tariff(new DateTime(2021, 12, 29, 14, 00, 00, DateTimeKind.Utc), 0, 0)},
                new DateTime(2021, 12, 29, 14, 00, 00, DateTimeKind.Utc),
                1
        };
    }
}