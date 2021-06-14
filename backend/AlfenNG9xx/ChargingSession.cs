using System;
using AlfenNG9xx.Model;
using EMS.Library.Adapter.EVSE;
using EMS.Library.TestableDateTime;

namespace AlfenNG9xx
{
    public class ChargingSession
    {
        private bool _isConnected = false;
        private double _meterReadingStart = 0;
        private DateTime _chargingStart;
        private bool _isCharging = false;

        public ChargeSessionInfoBase ChargeSessionInfo { get; private set; }

        public ChargingSession()
        {
            ChargeSessionInfo = DefaultSessionInfo();
        }

        public void UpdateSession(SocketMeasurement newMeasurement)
        {
            if (newMeasurement == null) return;

            // as long as the car is not connected, there is no session
            if (!_isConnected && !newMeasurement.VehicleConnected && newMeasurement.Mode3State == Mode3State.A)
            {
                ChargeSessionInfo = DefaultSessionInfo();
            }

            // start session as soon as the car is connected
            if (!_isConnected && newMeasurement.VehicleConnected)
            {
                _isConnected = true;
                _meterReadingStart = newMeasurement.RealEnergyDeliveredSum;
                ChargeSessionInfo = DefaultSessionInfo();
                ChargeSessionInfo.Start = DateTimeProvider.Now;
            }

            if (_isConnected)
            {
                ChargeSessionInfo.EnergyDelivered = newMeasurement.RealEnergyDeliveredSum - _meterReadingStart;
            }

            // stop session as soon as the car is no longer connected
            if (_isConnected && !newMeasurement.VehicleConnected)
            {
                _isConnected = false;
                ChargeSessionInfo.End = DateTimeProvider.Now;
                ChargeSessionInfo.SessionEnded = true;
            }

            // the car is started to charge; track start time
            if (newMeasurement.VehicleIsCharging && !_isCharging)
            {
                _chargingStart = DateTimeProvider.Now;
            }

            // the car has stopped charging; record the time
            if (!newMeasurement.VehicleIsCharging && _isCharging)
            {
                ChargeSessionInfo.ChargingTime += (uint)(DateTimeProvider.Now - _chargingStart).TotalSeconds;
            }            

            _isCharging = newMeasurement.VehicleIsCharging;
        }

        private static ChargeSessionInfoBase DefaultSessionInfo()
        {
            return new ChargeSessionInfoBase()
            {
                Start = null,
                End = null,
                ChargingTime = 0,
                EnergyDelivered = 0.0,
                SessionEnded = false
            };
        }
    }
}
