using System;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.TestableDateTime;
using Xunit;

namespace EMS.Library.Tests
{
	public class TariffTests
	{
        [Fact]
        public void TariffValidatesEqual()
        {
            var t1 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40), 0.1m, 0.1m);
            var t2 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40), 0.1m, 0.1m);
            Assert.Equal(t1, t2);            
        }

        [Fact]
        public void TariffNotEqualUsage()
        {
            var t1 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40), 0.1m, 0.1m);
            var t2 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40), 0.2m, 0.1m);
            Assert.NotEqual(t1, t2);
        }

        [Fact]
        public void TariffNotEqualReturn()
        {
            var t1 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40), 0.1m, 0.1m);
            var t2 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40), 0.1m, 0.2m);
            Assert.NotEqual(t1, t2);
        }

        [Fact]
        public void TariffNotEqualTimestamp()
        {
            var t1 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40), 0.1m, 0.1m);
            var t2 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 45), 0.1m, 0.1m);
            Assert.NotEqual(t1, t2);
        }

        [Fact]
        public void TariffNotEqualTimestampLocal()
        {
            var t1 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40, DateTimeKind.Local), 0.1m, 0.1m);
            var t2 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 45, DateTimeKind.Local), 0.1m, 0.1m);
            Assert.NotEqual(t1, t2);
        }

        [Fact]
        public void TariffNotEqualTimestampUTC()
        {
            var t1 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 40, DateTimeKind.Utc), 0.1m, 0.1m);
            var t2 = new Tariff(new DateTime(2022, 01, 09, 13, 35, 45, DateTimeKind.Utc), 0.1m, 0.1m);
            Assert.NotEqual(t1, t2);
        }
    }
}

