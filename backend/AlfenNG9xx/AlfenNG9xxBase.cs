using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

using EMS.Library;
using EMS.Library.Adapter.EVSE;
using EMS.Library.Adapter.PriceProvider;
using EMS.Library.Configuration;
using EMS.Library.Core;
using EMS.Library.Exceptions;
using EMS.Library.TestableDateTime;

using AlfenNG9xx.Model;

namespace AlfenNG9xx;

public abstract class AlfenBase : BackgroundWorker, IChargePoint
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    protected ChargingSession ChargingSession { get; init; } = new();

    public SocketMeasurementBase? LastSocketMeasurement { get; protected set; }
    public ChargeSessionInfoBase ChargeSessionInfo { get { return ChargingSession.ChargeSessionInfo; } }

    public event EventHandler<ChargingStatusUpdateEventArgs> ChargingStatusUpdate = delegate { };
    public event EventHandler<ChargingStateEventArgs> ChargingStateUpdate = delegate { };
    private readonly IPriceProvider _priceProvider;
    private readonly IWatchdog _watchdog;

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

    protected AlfenBase(InstanceConfiguration config, IPriceProvider priceProvider, IWatchdog watchdog)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(priceProvider);
        ArgumentNullException.ThrowIfNull(watchdog);
        Logger.Info($"Alfen({config.ToString().Replace(Environment.NewLine, " ", StringComparison.OrdinalIgnoreCase)})");
        _priceProvider = priceProvider;
        _watchdog = watchdog;
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

    protected override Task Start()
    {
#if DEBUG
        ShowProductInformation();
        ShowStationStatus();
        ShowSocketMeasurement();
#endif
        _watchdog.Register(this, 120);
        return base.Start();
    }

    protected override void Stop()
    {
        _watchdog.Unregister(this);
        base.Stop();
    }

    protected static async Task Delay(int millisecondsDelay, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(millisecondsDelay, stoppingToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException) { /* ignoring */ }
    }

    protected override DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(1250);
    }

    protected override Task DoBackgroundWork()
    {
        try
        {
            HandleWork();
            _watchdog.Tick(this);
        }
        catch (CommunicationException ce)
        {
            Logger.Error(ce, "Communication exception, we try later again\n");
        }
        return Task.CompletedTask;
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

    [SuppressMessage("Code Analysis", "CA1031")]
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

    [SuppressMessage("Code Analysis", "CA1031")]
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

