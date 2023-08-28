using Xunit;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.TestableDateTime;
using EMS.Library.Configuration;
using System.Diagnostics.CodeAnalysis;
using Moq;
using EMS.Library;

namespace EPEXSPOT.Unit.Tests;

public class TariffTests
{

    [Fact]
    [SuppressMessage("", "S1215")]
    public void DisposesProperly()
    {

        var mockFactory = new HttpClientFactoryMock();
        var w = new Mock<IWatchdog>();
        var mock = new Mock<EPEXSPOTService>(new InstanceConfiguration(), mockFactory, w.Object);
        mock.CallBase = true;

        mock.Object.Disposed.Should().BeFalse();
        
        mock.Object.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        mock.Object.Disposed.Should().BeTrue();
    }

    [Fact]
    [SuppressMessage("", "S1215")]
    public void DisposesCanSafelyCalledTwice()
    {
        var mockFactory = new HttpClientFactoryMock();
        var w = new Mock<IWatchdog>();
        var mock = new Mock<EPEXSPOTService>(new InstanceConfiguration(), mockFactory, w.Object);
        mock.CallBase = true;

        mock.Object.Disposed.Should().BeFalse();

        mock.Object.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        mock.Object.Disposed.Should().BeTrue();

        // and for the second time
        mock.Object.Dispose();
        mock.Object.Disposed.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.Correct), MemberType = typeof(TestDataGenerator))]
    public void FindsCorrectTariff(Tariff[] tariff, DateTime dateTime, int idx)
    {
        ArgumentNullException.ThrowIfNull(tariff);
        Tariff? t = EPEXSPOTService.FindTariff(tariff, dateTime);
        Assert.NotNull(t);
        Assert.Equal(tariff[idx].Timestamp, t.Timestamp);
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.NotAvailable), MemberType = typeof(TestDataGenerator))]
    public void TariffNotAvailable(Tariff[] tariff, DateTime dateTime)
    {
        var t = EPEXSPOTService.FindTariff(tariff, dateTime);
        Assert.Null(t);
    }


    [Fact]
    public void RemoveOldHandlesEmptyArray()
    {
        var result = EPEXSPOTService.RemoveOld(Array.Empty<Tariff>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveOldHandlesRemoval()
    {
        using (new DateTimeProviderContext(new DateTime(2023, 5, 1, 19, 0, 0, DateTimeKind.Utc)))
        {
            Tariff[] input = new Tariff[] {
                new Tariff(new DateTime(2023, 5, 1, 15, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 16, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 17, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 18, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 19, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 20, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 21, 0, 0, DateTimeKind.Utc), 0, 0)
            };
        
            var result = EPEXSPOTService.RemoveOld(input);
            result.Should().NotBeEmpty();
            result.Should().HaveCount(4);
        }
    }

    [Fact]
    public void RemoveOldHandlesRemovalAndSorts()
    {
        using (new DateTimeProviderContext(new DateTime(2023, 5, 1, 19, 0, 0, DateTimeKind.Utc)))
        {
            Tariff[] input = new Tariff[] {
                new Tariff(new DateTime(2023, 5, 1, 19, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 17, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 20, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 15, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 16, 0, 0, DateTimeKind.Utc), 0, 0),                
                new Tariff(new DateTime(2023, 5, 1, 18, 0, 0, DateTimeKind.Utc), 0, 0),
                new Tariff(new DateTime(2023, 5, 1, 21, 0, 0, DateTimeKind.Utc), 0, 0)
            };

            var result = EPEXSPOTService.RemoveOld(input);
            result.Should().NotBeEmpty();
            result.Should().HaveCount(4);
            result[0].Should().Be(input[5]);    //18:00
            result[1].Should().Be(input[0]);    //19:00
            result[2].Should().Be(input[2]);    //20:00
            result[3].Should().Be(input[6]);    //21:00
        }
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
                        new Tariff[] { new Tariff(new DateTime(2021, 12, 29, 13, 00, 00, DateTimeKind.Utc), 0, 0) },
                        new DateTime(2021, 12, 29, 12, 00, 00, DateTimeKind.Unspecified));
            t.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task ConvertsRawToTariff()
    {
        using var fileStream = File.OpenRead("TestData/TestData1.json");
        byte[] b = new byte[fileStream.Length];
        await fileStream.ReadAsync(b, CancellationToken.None).ConfigureAwait(false);

        var result = EPEXSPOTService.GetTariffFromRaw(b);
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task ConvertsRawToTariffWithProperValues()
    {
        using var fileStream = File.OpenRead("TestData/TestData1.json");
        byte[] b = new byte[fileStream.Length];
        await fileStream.ReadAsync(b, CancellationToken.None).ConfigureAwait(false);

        var result = EPEXSPOTService.GetTariffFromRaw(b);
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(4);
        
        var first = result[0];
        Assert.NotNull(first);
        first.Timestamp.Should<DateTime>().Be(new DateTime(2023, 05, 01, 19, 0, 0, DateTimeKind.Utc));
        first.TariffUsage.Should<Decimal>().BeGreaterThan(0);
        first.TariffReturn.Should<Decimal>().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertsRawToTariffReturnsNoDataWhenWrongJson()
    {
        using var fileStream = File.OpenRead("TestData/TestData_wrong_schema.json");
        byte[] b = new byte[fileStream.Length];
        await fileStream.ReadAsync(b, CancellationToken.None).ConfigureAwait(false);

        var result = EPEXSPOTService.GetTariffFromRaw(b);
        result.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ConvertsRawToTariffReturnsNoDataWhenInvalidJson()
    {
        using var fileStream = File.OpenRead("TestData/TestData_invalid_json.json");
        byte[] b = new byte[fileStream.Length];
        await fileStream.ReadAsync(b, CancellationToken.None).ConfigureAwait(false);

        var result = EPEXSPOTService.GetTariffFromRaw(b);
        result.Should().BeNullOrEmpty();
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

internal sealed class HttpClientFactoryMock : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        throw new NotImplementedException();
    }
}