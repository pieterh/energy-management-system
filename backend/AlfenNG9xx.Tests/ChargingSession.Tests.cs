using System;
using Xunit;

using AlfenNG9xx.Model;
using EMS.Library.Adapter.EVSE;
using EMS.Library.TestableDateTime;

namespace AlfenNG9xx.Tests
{
    public class ChargingSessionTests
    {
        [Fact]
        public void HandlesSimpleChargingSession()
        {
            var fakeDate = new DateTime(2021, 4, 1, 13, 14, 30);
            using (new DateTimeProviderContext(fakeDate))
            {
                var t = new ChargingSession();
                var sm = new SocketMeasurement
                {
                    Mode3State = Mode3State.E,
                    RealEnergyDeliveredSum = 1000
                };

                t.UpdateSession(sm);

                fakeDate = fakeDate.AddSeconds(30);
                using (new DateTimeProviderContext(fakeDate))
                {
                    sm.Mode3State = Mode3State.A;
                    t.UpdateSession(sm);

                    sm.Mode3State = Mode3State.B1;
                    t.UpdateSession(sm);

                    sm.Mode3State = Mode3State.B2;
                    t.UpdateSession(sm);

                    sm.Mode3State = Mode3State.C1;
                    t.UpdateSession(sm);

                    sm.Mode3State = Mode3State.C2;
                    t.UpdateSession(sm);

                    Assert.False(t.ChargeSessionInfo.SessionEnded);

                    fakeDate = fakeDate.AddSeconds(65);
                    using (new DateTimeProviderContext(fakeDate))
                    {
                        sm.Mode3State = Mode3State.E;
                        sm.RealEnergyDeliveredSum += 50;
                        t.UpdateSession(sm);

                        // session has ended, let's see the result of this simple charging session
                        Assert.Equal<uint>(65, t.ChargeSessionInfo.ChargingTime);
                        Assert.Equal<double>(50, t.ChargeSessionInfo.EnergyDelivered);
                        Assert.True(t.ChargeSessionInfo.SessionEnded);
                    }
                }
            }
        }

        [Fact]
        public void HandlesChargingSessionWithBreak()
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

                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.E);    // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.B1);   // 30 seconden op B1 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.B2);   // 30 seconden op B2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.C2);  // 30 seconden op C2 (charging)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.C1);   // 30 seconden op C1 (connected)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.C2);  // 30 seconden op C2 (charging)
                Assert.False(t.ChargeSessionInfo.SessionEnded);
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 0, Mode3State.E);    // 30 seconden op E  (disconnected)
                Assert.True(t.ChargeSessionInfo.SessionEnded);

                // session has ended, let's see the result of this simple charging session
                Assert.Equal<uint>(60, t.ChargeSessionInfo.ChargingTime);
                Assert.Equal<double>(100, t.ChargeSessionInfo.EnergyDelivered);
                
            }
        }

        [Fact]
        public void EnergyDeliveredNotWhenDisconnected()
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

                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E);  // 30 seconden op E  (disconnected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E);  // 30 seconden op E  (disconnected)
                Assert.Equal(0, t.ChargeSessionInfo.EnergyDelivered);
            }
        }

        [Fact]
        public void EnergyDeliveredWhenConnected()
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
                fakeDate = NextMeasurement(fakeDate, t, sm, 3600, 5000, Mode3State.B2);  // 1uur        op C2 (connected)
                fakeDate = NextMeasurement(fakeDate, t, sm, 30, 50, Mode3State.E);       // 30 seconden op E  (disconnected)
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
                fakeDate = NextMeasurement(fakeDate, t, sm, 3600, 5000, Mode3State.B2);  // 1uur        op C2 (connected)
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
            using (new DateTimeProviderContext(fakeDate))
            {
                sm.Mode3State = state;
                t.UpdateSession(sm);
            }
            fakeDate = fakeDate.AddSeconds(duration);
            using (new DateTimeProviderContext(fakeDate))
            {
                sm.RealEnergyDeliveredSum += energy;
                t.UpdateSession(sm);
            }
               
            return fakeDate;
        }
    }
}
