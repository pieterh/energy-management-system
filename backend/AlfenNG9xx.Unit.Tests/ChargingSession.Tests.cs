using System;
using Xunit;

using AlfenNG9xx.Model;
using EMS.Library.Adapter.EVSE;
using EMS.Library.TestableDateTime;
using EMS.Library.Adapter.PriceProvider;

namespace AlfenNG9xx.Tests
{
    public class ChargingSessionTests
    {
        [Fact]
        public void HandlesSimpleChargingSession()
        {
            var fakeDate = new DateTime(2021, 4, 1, 13, 14, 30);
            var tariff = new Tariff(fakeDate, 0.23m, 0.08m);

            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                t.UpdateSession(sm, tariff);

                fakeDate = fakeDate.AddSeconds(30);
                using (new DateTimeProviderContext(fakeDate))
                {
                    sm.Mode3State = Mode3State.A;
                    t.UpdateSession(sm, tariff);

                    sm.Mode3State = Mode3State.B1;
                    t.UpdateSession(sm, tariff);

                    sm.Mode3State = Mode3State.B2;
                    t.UpdateSession(sm, tariff);

                    sm.Mode3State = Mode3State.C1;
                    t.UpdateSession(sm, tariff);

                    sm.Mode3State = Mode3State.C2;
                    t.UpdateSession(sm, tariff);

                    Assert.False(t.ChargeSessionInfo.SessionEnded);

                    fakeDate = fakeDate.AddSeconds(65);
                    using (new DateTimeProviderContext(fakeDate))
                    {
                        sm.Mode3State = Mode3State.E;
                        sm.RealEnergyDeliveredSum += 50000;
                        t.UpdateSession(sm, tariff);

                        // session has ended, let's see the result of this simple charging session
                        Assert.Equal<uint>(65, t.ChargeSessionInfo.ChargingTime);
                        Assert.Equal<double>(50000, t.ChargeSessionInfo.EnergyDelivered);
                        Assert.True(t.ChargeSessionInfo.SessionEnded);
                        Assert.Equal<Decimal>(11.5m, t.ChargeSessionInfo.Cost);
                    }
                }
            }
        }

        [Fact]
        public void HandlesChargingSessionWithBreak()
        {
            var fakeDate = new DateTime(2021, 4, 1, 13, 14, 00);
            var tariff1 = new Tariff(fakeDate, 0.215m, 0.08m);
            var tariff2 = new Tariff(fakeDate, 0.254m, 0.08m);

            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.E, tariff1);    // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.B1, tariff1);   // 30 seconden op B1 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.B2, tariff1);   // 30 seconden op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 25000, Mode3State.C2, tariff1);  // 30 seconden op C2 (charging)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                //Assert.Equal<Decimal>(5.375m, t.ChargeSessionInfo.Cost);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.C1, tariff1);   // 30 seconden op C1 (connected)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 20000, Mode3State.C2, tariff2);  // 30 seconden op C2 (charging)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.E, tariff2);    // 30 seconden op E  (disconnected)
                Assert.True(t.ChargeSessionInfo.SessionEnded);

                // session has ended, let's see the result of this simple charging session
                Assert.Equal<uint>(60, t.ChargeSessionInfo.ChargingTime);
                Assert.Equal<double>(45000, t.ChargeSessionInfo.EnergyDelivered);
                Assert.Equal<Decimal>(10.455m, t.ChargeSessionInfo.Cost);
            }
        }

        [Fact]
        public void EnergyDeliveredNotWhenDisconnected()
        {
            var fakeDate = new DateTime(2021, 4, 1, 13, 14, 00);
            var tariff = new Tariff(fakeDate, 0.23m, 0.08m);
            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E, tariff);  // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E, tariff);  // 30 seconden op E  (disconnected)
                Assert.Equal(0, t.ChargeSessionInfo.EnergyDelivered);
            }
        }

        // uh?
        [Fact]
        public void EnergyDeliveredWhenConnected()
        {
            var fakeDate = new DateTime(2021, 4, 1, 13, 14, 00);
            var tariff = new Tariff(fakeDate, 0.23m, 0.08m);

            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E, tariff);       // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 10, Mode3State.B1, tariff);      // 30 seconden op B1 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 10, Mode3State.B2, tariff);      // 30 seconden op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 3600, 5000, Mode3State.B2, tariff);  // 1uur        op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E, tariff);       // 30 seconden op E  (disconnected)
                Assert.Equal(5020, t.ChargeSessionInfo.EnergyDelivered);
                Assert.True(t.ChargeSessionInfo.SessionEnded);
            }
        }

        [Fact]
        public void SessionResetWhenStandby()
        {
            var fakeDate = new DateTime(2021, 4, 1, 13, 14, 00);
            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E);       // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 10, Mode3State.B1);      // 30 seconden op B1 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 10, Mode3State.B2);      // 30 seconden op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 3600, 5000, Mode3State.B2);  // 1uur        op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E);       // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.A);       // 30 seconden op A  (standby)
                Assert.Null(t.ChargeSessionInfo.Start);
                Assert.Null(t.ChargeSessionInfo.End);
                Assert.Equal(0, t.ChargeSessionInfo.EnergyDelivered);
                Assert.Equal((uint)0, t.ChargeSessionInfo.ChargingTime);
                Assert.False(t.ChargeSessionInfo.SessionEnded); 
            }
        }


        private static DateTime NextMeasurement(DateTime fakeDate, ChargingSession t, SocketMeasurement sm, int duration, double energy, Mode3State state)
        {
            return NextMeasurement(fakeDate, t, sm, duration, energy, state, null);
        }

        private static DateTime NextMeasurement(DateTime fakeDate, ChargingSession t, SocketMeasurement sm, int duration, double energy, Mode3State state, Tariff? tariff)
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
