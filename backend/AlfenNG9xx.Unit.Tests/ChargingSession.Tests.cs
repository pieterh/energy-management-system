using System;
using Xunit;
using FluentAssertions;

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
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.C1, tariff1);   // 30 seconden op C1 (connected)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 20000, Mode3State.C2, tariff2);  // 30 seconden op C2 (charging)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                _ = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.E, tariff2);    // 30 seconden op E  (disconnected)
                Assert.True(t.ChargeSessionInfo.SessionEnded);

                // session has ended, let's see the result of this simple charging session
                Assert.Equal<uint>(60, t.ChargeSessionInfo.ChargingTime);
                Assert.Equal<double>(45000, t.ChargeSessionInfo.EnergyDelivered);
                Assert.Equal<Decimal>(10.455m, t.ChargeSessionInfo.Cost);
            }
        }


        [Fact]
        public void HandlesChargingSessionWithBreaksAndButtonDisco()
        {
            var fakeDate = new DateTime(2023, 6, 10, 00, 51, 46);
            var tariff1 = new Tariff(fakeDate, 0.20m, 0.08m);
            var tariff2 = new Tariff(fakeDate, 0.19m, 0.08m);

            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                fakeDate = NextMeasurement(fakeDate, t, sm, 60, 0, Mode3State.E, tariff1);                      //  60 seconden op E (No power (shut off))

                fakeDate = NextMeasurement(fakeDate, t, sm, (00 * 60) + 02, 0, Mode3State.A, tariff1);          //   2 seconden op A (Standby)
                fakeDate = NextMeasurement(fakeDate, t, sm, (00 * 60) + 02, 0, Mode3State.B1, tariff1);         //   2 seconden op B1 (Vehicle detected)

                fakeDate = NextMeasurement(fakeDate, t, sm, (64 * 60) + 02, 68, Mode3State.C2, tariff1);     //  64 minuten en 2 seconden op C2 (Charging (PWM signal applied))
                fakeDate = NextMeasurement(fakeDate, t, sm, (180 * 60) + 02, 0, Mode3State.B2, tariff1);    // 180 minuten en 2 seconden op B2 (Vehicle detected (PWM signal applied))

                fakeDate = NextMeasurement(fakeDate, t, sm, (59 * 60) + 02, 10960, Mode3State.C2, tariff1);     // 180 minuten en 2 seconden op C2 (Charging (PWM signal applied))
                fakeDate = NextMeasurement(fakeDate, t, sm, (59 * 60) + 02, 0, Mode3State.B2, tariff1);     //  59 minuten en 2 seconden op B2 (Vehicle detected (PWM signal applied))

                fakeDate = NextMeasurement(fakeDate, t, sm, (59 * 60) + 02, 11040, Mode3State.C2, tariff1);     //  64 minuten en 2 seconden op C2 (Charging (PWM signal applied))
                fakeDate = NextMeasurement(fakeDate, t, sm, (00 * 60) + 20, 0, Mode3State.B2, tariff1);     //   0 minuten en 20 seconden op B2 (Vehicle detected (PWM signal applied))

                fakeDate = NextMeasurement(fakeDate, t, sm, (00 * 60) + 05, 85, Mode3State.C2, tariff1);     // 0 minuten en 5 seconden op C2 (Charging (PWM signal applied))
                fakeDate = NextMeasurement(fakeDate, t, sm, (00 * 60) + 58, 0, Mode3State.B2, tariff1);     //  0 min en 58 seconden op B2

                fakeDate = NextMeasurement(fakeDate, t, sm, (24 * 60) + 40, 3600, Mode3State.C2, tariff1);     // 24 min en 40 seconden op C2 (charging)
                fakeDate = NextMeasurement(fakeDate, t, sm, (16 * 60) + 02, 0, Mode3State.B2, tariff1);     // 16 min en  2 seconden op B2 

                using (new DateTimeProviderContext(fakeDate))
                {
                    sm.Mode3State = Mode3State.A;
                    t.UpdateSession(sm, tariff1);
                }

                //fakeDate = NextMeasurement(fakeDate, t, sm, 10, 0, Mode3State.A, tariff1);                  // 10 seconden op A (Standby)
                //fakeDate = NextMeasurement(fakeDate, t, sm, 2, 0, Mode3State.B1, tariff1);                  // 2 seconden op B1 (Vehicle detected)

                //_ = NextMeasurement(fakeDate, t, sm, 60, 0, Mode3State.E, tariff1);                         // 60 seconden op (No power (shut off))

                t.ChargeSessionInfo.SessionEnded.Should().Be(true);
                // session has ended, let's see the result of this simple charging session
                t.ChargeSessionInfo.ChargingTime.Should().Be(12411);
                t.ChargeSessionInfo.EnergyDelivered.Should().Be(25753);
                t.ChargeSessionInfo.Cost.Should().Be(5.15060m);
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
                _ = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E, tariff);  // 30 seconden op E  (disconnected)
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
                _ = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E, tariff);       // 30 seconden op E  (disconnected)
                Assert.Equal(5020, t.ChargeSessionInfo.EnergyDelivered);
                Assert.True(t.ChargeSessionInfo.SessionEnded);
            }
        }

        [Fact]
        public void NoTariffAvailWhenCharging()
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

                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.E, tariff);       // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.B1, tariff);      // 30 seconden op B1 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.B2, tariff);      // 30 seconden op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 40, 25000, Mode3State.C2, tariff);  // 40 seconden op C2 (charging)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 25000, Mode3State.C2, null);    // 30 seconden op C2 (charging) (no tariff available)
                fakeDate = NextMeasurement(fakeDate, t, sm, 20, 25000, Mode3State.C2, null);    // 20 seconden op C2 (charging) (no tariff available)
                fakeDate = NextMeasurement(fakeDate, t, sm, 10, 25000, Mode3State.C2, tariff);  // 10 seconden op C2 (charging) (tariff available)
                _ = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E, tariff);      // 30 seconden op E  (disconnected)

                Assert.Equal(100000, t.ChargeSessionInfo.EnergyDelivered);
                Assert.True(t.ChargeSessionInfo.SessionEnded);
                t.ChargeSessionInfo.Costs.Count.Should().Be(2); // two different tariffs
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
                _ = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.A);       // 30 seconden op A  (standby)
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
