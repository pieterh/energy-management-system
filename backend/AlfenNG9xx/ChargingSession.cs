using System;
using AlfenNG9xx.Model;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.TestableDateTime;

namespace AlfenNG9xx
{
    public class ChargingSession
    {
        private bool _isConnected = false;
        private double _meterReadingStart = 0;
        private DateTime _chargingStart;
        private bool _isCharging = false;

        private double _cost = 0.0d;
        private Tariff _currentTariff = null;
        private double _meterReadingStartTariff = 0;
        

        public ChargeSessionInfoBase ChargeSessionInfo { get; private set; }

        public ChargingSession()
        {
            ChargeSessionInfo = DefaultSessionInfo();
        }

        public void UpdateSession(SocketMeasurement newMeasurement, Tariff newTariff)
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

                // reset cost calculation
                _cost = 0.0d;
                _currentTariff = newTariff;
                _meterReadingStartTariff = newMeasurement.RealEnergyDeliveredSum;
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

            // we are charging and the tariff did change
            // calculate the costs for the usage in for the last tariff
            if (_isCharging && _currentTariff != null)
            {
                if (_currentTariff.TariffUsage != newTariff.TariffUsage)
                {
                    _cost += (newMeasurement.RealEnergyDeliveredSum - _meterReadingStartTariff) * _currentTariff.TariffUsage;

                    ChargeSessionInfo.Cost = _cost;
                    _meterReadingStartTariff = newMeasurement.RealEnergyDeliveredSum;
                    _currentTariff = newTariff;
                }
                else
                {
                    ChargeSessionInfo.Cost = _cost + ((newMeasurement.RealEnergyDeliveredSum - _meterReadingStartTariff) * _currentTariff.TariffUsage);
                }
            }

            // the car is started to charge; track start time, tariff
            if (newMeasurement.VehicleIsCharging && !_isCharging)
            {
                _chargingStart = DateTimeProvider.Now;

                _currentTariff = newTariff;
                _meterReadingStartTariff = newMeasurement.RealEnergyDeliveredSum;
            }

            // the car has stopped charging; record the time
            if (!newMeasurement.VehicleIsCharging && _isCharging)
            {
                ChargeSessionInfo.ChargingTime += (uint)(DateTimeProvider.Now - _chargingStart).TotalSeconds;

                _cost += (newMeasurement.RealEnergyDeliveredSum - _meterReadingStartTariff) * _currentTariff.TariffUsage;
                ChargeSessionInfo.Cost = _cost;                
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
