using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AlfenNG9xx.Model;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Core;
using EMS.Library.TestableDateTime;

namespace AlfenNG9xx
{
    public class ChargingSession
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _isConnected;
        private double _meterReadingStart;
        private DateTimeOffset _chargingStart;
        private bool _isCharging;

        private DateTimeOffset _currentTariffDateTime;
        private Tariff? _currentTariff;
        private double _meterReadingStartTariff;

        public SocketMeasurementBase? LastSocketMeasurement { get; protected set; }
        public ChargeSessionInfoBase ChargeSessionInfo { get; private set; } = DefaultSessionInfo();

        public void UpdateSession(SocketMeasurement newMeasurement, Tariff? newTariff)
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
                ChargeSessionInfo.Start = DateTimeOffsetProvider.Now;

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
                ChargeSessionInfo.End = DateTimeOffsetProvider.Now;
                ChargeSessionInfo.SessionEnded = true;
            }

            // the car is started to charge; track start time, tariff
            if (newMeasurement.VehicleIsCharging && !_isCharging)
            {
                _chargingStart = DateTimeProvider.Now;

                _currentTariffDateTime = DateTimeOffsetProvider.Now;
                _currentTariff = newTariff;
                _meterReadingStartTariff = newMeasurement.RealEnergyDeliveredSum;
            }

            // the car has stopped charging; record the time
            if (!newMeasurement.VehicleIsCharging && _isCharging)
            {
                ChargeSessionInfo.ChargingTime += (uint)(DateTimeProvider.Now - _chargingStart).TotalSeconds;

                // nog te doen: gebruik moment van gebruik van dit tarief ipv now
                ChargeSessionInfo.Costs.Add(new Cost(_currentTariffDateTime, _currentTariff, (newMeasurement.RealEnergyDeliveredSum - _meterReadingStartTariff)));
                ChargeSessionInfo.RunningCost = 0m;
            }

            _isCharging = newMeasurement.VehicleIsCharging;

            // we are charging and the tariff did change
            // calculate the costs for the usage in for the last tariff
            if (_isCharging && _currentTariff != null)
            {
                if (!_currentTariff.Equals(newTariff))
                {
                    // nog te doen: gebruik moment van gebruik van dit tarief ipv now
                    ChargeSessionInfo.Costs.Add(new Cost(_currentTariffDateTime, _currentTariff, (newMeasurement.RealEnergyDeliveredSum - _meterReadingStartTariff)));
                    ChargeSessionInfo.RunningCost = 0m;

                    _currentTariffDateTime = DateTimeProvider.Now;
                    _currentTariff = newTariff;
                    _meterReadingStartTariff = newMeasurement.RealEnergyDeliveredSum;
                }
                else
                {
                    // nog te doen: hoe tussentijds
                    var energy = (decimal)(newMeasurement.RealEnergyDeliveredSum - _meterReadingStartTariff);
                    var costPerWatt = _currentTariff.TariffUsage > 0 ? _currentTariff.TariffUsage / 1000.0m : 0.0m;
                    ChargeSessionInfo.RunningCost = energy * costPerWatt;
                }
            }

            RaiseEvents(newMeasurement);

            LastSocketMeasurement = newMeasurement;
        }

        public event EventHandler<ChargingStatusUpdateEventArgs> ChargingStatusUpdate = delegate { };
        [SuppressMessage("", "CA1030")]
        protected void RaiseChargingStatusUpdateEvent(ChargingStatusUpdateEventArgs eventArgs)
        {
            ChargingStatusUpdate.Invoke(this, eventArgs);
        }

        public event EventHandler<ChargingStateEventArgs> ChargingStateUpdate = delegate { };
        [SuppressMessage("", "CA1030")]
        protected void RaiseChargingStateEvent(ChargingStateEventArgs eventArgs)
        {
            ChargingStateUpdate.Invoke(this, eventArgs);
        }

        internal void RaiseEvents(SocketMeasurement newMeasurement)
        {
            
            RaiseChargingStatusUpdateEvent(new ChargingStatusUpdateEventArgs(newMeasurement));

            var chargingStateChanged = LastSocketMeasurement?.Mode3State != newMeasurement.Mode3State;

            if (chargingStateChanged)
            {
                var sessionStart = ChargeSessionInfo.Start;
                var sessionEnd = ChargeSessionInfo.End;
                var sessionEnded = ChargeSessionInfo.SessionEnded;
                var energyDelivered = ChargeSessionInfo.EnergyDelivered;
                var cost = ChargeSessionInfo.Cost;
                var costs = ChargeSessionInfo.Costs;

                foreach (Cost c in ChargeSessionInfo.Costs)
                {
                    Logger.Debug("Cost : {0}, {1}, {2}", c.Timestamp.ToLocalTime().ToString("O"), c.Energy, c.Tariff?.TariffUsage);
                }
                RaiseChargingStateEvent(new ChargingStateEventArgs(newMeasurement, sessionEnded, sessionStart, sessionEnd, energyDelivered, cost, costs));
            }
        }

        private static ChargeSessionInfoBase DefaultSessionInfo()
        {
            return new ChargeSessionInfoBase()
            {
                Start = DateTimeOffset.MinValue,
                End = DateTimeOffset.MinValue,
                ChargingTime = 0,
                EnergyDelivered = 0.0,
                SessionEnded = false,
                Costs = new List<Cost>()
            };
        }
    }
}
