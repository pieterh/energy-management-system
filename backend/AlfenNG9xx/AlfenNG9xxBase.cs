using System.Diagnostics.CodeAnalysis;
using AlfenNG9xx.Model;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Configuration;
using EMS.Library.Core;
using EMS.Library.Exceptions;

namespace AlfenNG9xx
{
    public abstract class AlfenBase : Microsoft.Extensions.Hosting.BackgroundService, IChargePoint
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed;
        internal bool isDisposed { get { return _disposed; } }

        protected ChargingSession ChargingSession { get; init; } = new();

        public SocketMeasurementBase? LastSocketMeasurement { get; protected set; }
        public ChargeSessionInfoBase ChargeSessionInfo { get { return ChargingSession.ChargeSessionInfo; } }

        public event EventHandler<ChargingStatusUpdateEventArgs> ChargingStatusUpdate = delegate { };
        public event EventHandler<ChargingStateEventArgs> ChargingStateUpdate = delegate { };
        private readonly IPriceProvider _priceProvider;

        protected abstract void PerformUpdateMaxCurrent(double maxCurrent, ushort phases);
        public abstract ProductInformation ReadProductInformation();
        public abstract EMS.Library.StationStatus ReadStationStatus();
        public abstract SocketMeasurement ReadSocketMeasurement(byte socket);

        [SuppressMessage("", "CA1030")]
        protected void RaiseChargingStatusUpdateEvent(ChargingStatusUpdateEventArgs eventArgs)
        {
            ChargingStatusUpdate.Invoke(this, eventArgs);
        }

        [SuppressMessage("", "CA1030")]
        protected void RaiseChargingStateEvent(ChargingStateEventArgs eventArgs)
        {
            ChargingStateUpdate.Invoke(this, eventArgs);
        }

        protected AlfenBase(Config config, IPriceProvider priceProvider)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(priceProvider);
            Logger.Info($"Alfen({config.ToString().Replace(Environment.NewLine, " ", StringComparison.OrdinalIgnoreCase)})");
            _priceProvider = priceProvider;
        }

        /// <summary>
        /// IChargePoint
        /// </summary>
        /// <param name="maxCurrent"></param>
        /// <param name="phases"></param>
        public void UpdateMaxCurrent(double maxCurrent, ushort phases)
        {
            Logger.Info($"UpdateMaxCurrent({maxCurrent}, {phases})");
            if (maxCurrent <= 0)
                return;

            PerformUpdateMaxCurrent(maxCurrent, phases);
        }

        /// <summary>
        /// IChargePoint
        /// </summary>
        /// <param name="maxL1"></param>
        /// <param name="maxL2"></param>
        /// <param name="maxL3"></param>
        public void UpdateMaxCurrent(double maxL1, double maxL2, double maxL3)
        {
            Logger.Info($"UpdateMaxCurrent({maxL1}, {maxL2}, {maxL3})");
            (double maxCurrent, ushort phases) = DetermineMaxCurrent(maxL1, maxL2, maxL3);
            if (maxCurrent <= 0)
                return;
            UpdateMaxCurrent(maxCurrent, phases);
        }

        public static (double, ushort) DetermineMaxCurrent(double maxL1, double maxL2, double maxL3)
        {
            Logger.Info($"UpdateMaxCurrent({maxL1}, {maxL2}, {maxL3})");

            if (maxL1 < 0f && maxL2 < 0f && maxL3 < 0f) return (-1, 0);
            ushort phases;
            float maxCurrent;
            if (maxL2 <= 0f || maxL3 <= 0f)
            {
                phases = 1;
                maxCurrent = (float)Math.Round(maxL1, 1, MidpointRounding.ToZero);
            }
            else
            {
                phases = 3;
                maxCurrent = (float)Math.Round(Math.Min(maxL1, Math.Min(maxL2, maxL3)), 1, MidpointRounding.ToZero);
            }

            return (maxCurrent, phases);
        }

        public override void Dispose()
        {
            Dispose(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        // Unfortunately the base class doesn't implement the dispose pattern
        // properly, so there is no method Dispose(bool disposing) to override...
        // https://github.com/dotnet/runtime/issues/34809
        // Therefor we need to suppress the sonar message
        [SuppressMessage("Sonar", "S2953")]
        internal virtual void Dispose(bool disposing)
        {
            Logger.Trace("Dispose({Disposing}) _disposed {Disposed}", disposing, _disposed);

            if (_disposed) return;

            _disposed = true;
            Logger.Trace("Dispose({Disposing}) done => _disposed {Disposed}", disposing, _disposed);
        }

        protected static async Task Delay(int millisecondsDelay, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(millisecondsDelay, stoppingToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { /* ignoring */ }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("Alfen NG9xx - Starting");

#if DEBUG
            ShowProductInformation();
            ShowStationStatus();
            ShowSocketMeasurement();
#endif

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    HandleWork();
                    await Task.Delay(1250, stoppingToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException tce)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        Logger.Error("Exception: " + tce.Message);
                    }
                }
                catch (CommunicationException ce)
                {
                    Logger.Error(ce, "Communication exception, we try later again\n");
                    await Delay(2500, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Unhandled, we try later again\n");
                    await Delay(2500, stoppingToken).ConfigureAwait(false);
                }
            }

            Logger.Info("Canceled");
            stoppingToken.ThrowIfCancellationRequested();
        }

        internal protected void HandleWork()
        {
            var sm = ReadSocketMeasurement(1);
            if (sm == null) return;

            var tariff = _priceProvider.GetTariff();
            ChargingSession.UpdateSession(sm, tariff);

            var chargingStateChanged = LastSocketMeasurement?.Mode3State != sm.Mode3State;

            LastSocketMeasurement = sm;
            RaiseChargingStatusUpdateEvent(new ChargingStatusUpdateEventArgs(sm));

            if (chargingStateChanged)
            {
                var sessionEnded = ChargingSession.ChargeSessionInfo.SessionEnded;
                var energyDelivered = ChargingSession.ChargeSessionInfo.EnergyDelivered;
                var cost = ChargingSession.ChargeSessionInfo.Cost;
                var costs = ChargingSession.ChargeSessionInfo.Costs;

                foreach (Cost c in ChargingSession.ChargeSessionInfo.Costs)
                {
                    Logger.Debug("Cost : {0}, {1}, {2}", c.Timestamp.ToLocalTime().ToString("O"), c.Energy, c.Tariff?.TariffUsage);
                }
                RaiseChargingStateEvent(new ChargingStateEventArgs(sm, sessionEnded, energyDelivered, cost, costs));
            }
        }

        internal static string PlatformTypeToModel(string platformType) => platformType switch
        {
            "NG900" => "Alfen Eve Single S-line",
            "NG910" => "Alfen Eve Single Pro-line",
            "NG920" => "Alfen Eve Double Pro-line / Eve Double PG / Twin 4XL",
            _ => $"Unknown platform type {platformType}"
        };

#if DEBUG
        protected void ShowProductInformation()
        {
            var pi = ReadProductInformation();
            Logger.Info(pi.ToPrintableString());
        }

        protected void ShowStationStatus()
        {
            try
            {
                var status = ReadStationStatus();
                if (status == null) return;

                Logger.Info($"Station Active Max Current : {status.ActiveMaxCurrent}");
                Logger.Info($"Temperature                : {status.Temperature}");
                Logger.Info($"OCCP                       : {status.OCCPState}");
                Logger.Info($"Nr of sockets              : {status.NrOfSockets}");
            }
            catch (SystemException se)
            {
                Logger.Error(se, $"{se.Message}");
            }
        }

        protected void ShowSocketMeasurement()
        {
            try
            {
                var sm = ReadSocketMeasurement(1);
                if (sm == null) return;

                Logger.Info($"Meter State                : {sm.MeterState}");
                Logger.Info($"Meter Timestamp            : {sm.MeterTimestamp}");
                Logger.Info($"Meter Type                 : {sm.MeterType}");

                Logger.Info($"Real Energy Delivered L1   : {sm.RealEnergyDeliveredL1}");
                Logger.Info($"Real Energy Delivered L2   : {sm.RealEnergyDeliveredL2}");
                Logger.Info($"Real Energy Delivered L3   : {sm.RealEnergyDeliveredL3}");
                Logger.Info($"Real Energy Delivered Sum  : {sm.RealEnergyDeliveredSum}");

                Logger.Info($"Availability               : {sm.Availability}");
                Logger.Info($"Mode 3 State               : {sm.Mode3State}");

                Logger.Info($"Actual Applied Max Current : {sm.AppliedMaxCurrent}");
                Logger.Info($"Max Current Valid Time     : {sm.MaxCurrentValidTime}");
                Logger.Info($"Max Current                : {sm.MaxCurrent}");
                Logger.Info($"Active Load Bl safe current: {sm.ActiveLBSafeCurrent}");
                Logger.Info($"Setpoint accounted for     : {sm.SetPointAccountedFor}");
                Logger.Info($"Using # phases             : {sm.Phases}");
            }
            catch (SystemException se)
            {
                Logger.Error(se, $"{se.Message}");
            }
        }
#endif
    }
}

