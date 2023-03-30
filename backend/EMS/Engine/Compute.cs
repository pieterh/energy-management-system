using System;
using System.Linq;
using Microsoft.Extensions.Logging;

using EMS.Engine;
using EMS.Engine.Model;
using EMS.Library.Adapter;
using EMS.Library.Core;

namespace EMS
{
    public class Compute
    {
        private readonly Measurements _measurements = new(60);
        private readonly ChargingStateMachine _state = new();

        // nog te doen refactor
        private Base _model;
        private readonly MaxCharging _maxCharging;
        private readonly EcoFriendly _ecoFriendly;
        private readonly MaxSolar _maxSolar;
        private readonly SlowCharge _slowCharge;

        public ushort MinimumDataPoints
        {
            get { return _model.MinimumDataPoints; }
        }

        public ushort MaxBufferSeconds
        {
            get { return _model.MaxBufferSeconds; }  // 15min or 1min buffer size
        }

        private ChargingMode _mode;
        public ChargingMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                switch (_mode)
                {
                    case ChargingMode.MaxCharge:
                        _model = _maxCharging;
                        break;
                    case ChargingMode.MaxEco:
                        _model = _ecoFriendly;
                        break;
                    case ChargingMode.MaxSolar:
                        _model = _maxSolar;
                        break;
                    case ChargingMode.SlowCharge:
                        _model = _slowCharge;
                        break;
                }

                // changing the mode also means that the buffer size is going to change
                _measurements.BufferSeconds = _model.MaxBufferSeconds;
                if (_mode == ChargingMode.MaxEco) _state.MinimumTransitionTime = 0; else _state.MinimumTransitionTime = ChargingStateMachine.DEFAULT_MINIMUM_TRANSITION_TIME;
            }
        }

        private ChargeControlInfo _info;
        public ChargeControlInfo Info
        {
            get { return _info; }
            private set
            {
                var org = _info;
                _info = value;

                if (org != null && _info != null)
                {
                    if ((org.Mode != _info.Mode) || (org.State != _info.State))
                    {
                        RaiseStateUpdate(_info);
                    }
                }
                else
                {
                    if (org != null || _info != null)
                        RaiseStateUpdate(_info);
                }
            }
        }

        public event EventHandler<StateUpdateEventArgs> StateUpdate;

        public Compute(ILogger logger, ChargingMode mode)
        {
            _maxCharging = new(logger, _measurements, _state);
            _ecoFriendly = new(logger, _measurements, _state);
            _maxSolar = new(logger, _measurements, _state);
            _slowCharge = new(logger, _measurements, _state);

            Mode = mode;
            Info = new();
        }

        public (double l1, double l2, double l3) Charging()
        {
            if (_model == null) return (-1, -1, -1);
            var t = _model.GetCurrent();
            Info = new ChargeControlInfo(Mode, _state.Current, _state.LastStateChange, t.l1, t.l2, t.l3, _measurements.GetMeasurements());
            return t;
        }

        public void AddMeasurement(ICurrentMeasurement m, ICurrentMeasurement sm)
        {
            _measurements.AddData(m?.CurrentL1, m?.CurrentL2, m?.CurrentL3, sm?.CurrentL1, sm?.CurrentL2, sm?.CurrentL3);
        }

        private void RaiseStateUpdate(ChargeControlInfo nfo)
        {
            StateUpdate?.Invoke(this, new StateUpdateEventArgs(nfo));
        }
    }

    public class StateUpdateEventArgs : EventArgs
    {
        public ChargeControlInfo Info { get; set; }

        public StateUpdateEventArgs(ChargeControlInfo nfo)
        {
            Info = nfo;
        }
    }
}
