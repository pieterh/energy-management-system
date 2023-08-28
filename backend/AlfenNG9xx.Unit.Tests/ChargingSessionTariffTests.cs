using System;
using AlfenNG9xx.Model;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.TestableDateTime;
using Xunit;

namespace AlfenNG9xx.Tests
{
	public class ChargingSessionTariffTests
	{
        [Fact]
        public void HandlesChangeInTariff()
        {
            var fakeDate = new DateTime(2021, 4, 1, 13, 14, 00, DateTimeKind.Utc);

            var tariff1 = new Tariff(new DateTime(2021, 4, 1, 13, 00, 00, DateTimeKind.Utc), 0.11m, 0.01m);
            var tariff2 = new Tariff(new DateTime(2021, 4, 1, 13, 30, 00, DateTimeKind.Utc), 0.12m, 0.02m);
            var tariff3 = new Tariff(new DateTime(2021, 4, 1, 14, 00, 00, DateTimeKind.Utc), 0.13m, 0.03m);
            var tariff4 = new Tariff(new DateTime(2021, 4, 1, 14, 30, 00, DateTimeKind.Utc), 0.14m, 0.04m);

            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                fakeDate = NextMeasurement(fakeDate, t, sm, 10, 0, Mode3State.E, tariff1);          // 10 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 15, 0, Mode3State.B1, tariff1);         // 15 seconden op B1 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 05, 0, Mode3State.B2, tariff1);         // 30 seconden op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 16*60, 11000, Mode3State.C2, tariff1);     // 16 min op C2 (charging)
                Assert.Equal<Decimal>(1.21m, t.ChargeSessionInfo.Cost);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30*60, 12000, Mode3State.C2, tariff2);     // 30 min op C2 (charging)
                Assert.Equal<Decimal>(2.65m, t.ChargeSessionInfo.Cost);
                fakeDate = NextMeasurement(fakeDate, t, sm, 20*60, 9000, Mode3State.C2, tariff3);      // 20 min op C2 (charging)
                Assert.Equal<Decimal>(3.82m, t.ChargeSessionInfo.Cost);
                fakeDate = NextMeasurement(fakeDate, t, sm, 10*60, 4000, Mode3State.C2, tariff3);      // 10 min op C2 (charging)
                Assert.Equal<Decimal>(4.34m, t.ChargeSessionInfo.Cost);
                _ = NextMeasurement(fakeDate, t, sm, 60, 0, Mode3State.E, tariff4);          // 1 min op E (disconnected)
                Assert.True(t.ChargeSessionInfo.SessionEnded);

                // session has ended, let's see the result of this simple charging session
                Assert.Equal<uint>(76*60, t.ChargeSessionInfo.ChargingTime);
                Assert.Equal<double>(36000, t.ChargeSessionInfo.EnergyDelivered);
                Assert.Equal<Decimal>(4.34m, t.ChargeSessionInfo.Cost);
            }
        }

        private static DateTime NextMeasurement(DateTime fakeDate, ChargingSession t, SocketMeasurement sm, int duration, double energy, Mode3State state, Tariff tariff)
        {
            using (new DateTimeProviderContext(fakeDate))
            {
                sm.Mode3State = state;
                t.UpdateSession(sm, tariff);
            }
            fakeDate = fakeDate.AddSeconds(duration);
            using (new DateTimeProviderContext(fakeDate))
            {
                sm.RealEnergyDeliveredSum += energy;
                t.UpdateSession(sm, tariff);
            }

            return fakeDate;
        }
    }
}

