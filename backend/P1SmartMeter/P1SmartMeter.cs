using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using EMS.Library;
using EMS.Library.Adapter.SmartMeterAdapter;
using EMS.Library.Configuration;
using EMS.Library.TestableDateTime;

using P1SmartMeter.Connection;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter;

public class SmartMeter : BackgroundWorker, ISmartMeterAdapter
{
    private readonly ILogger Logger;

    private enum ConnectionType { LAN, TTY };
    private readonly ConnectionType _connectionType;
    private readonly string? _host;
    private readonly int? _port;
    private readonly string? _usbPort;
    private readonly MessageBuffer _buffer;

    /*** Dataflow ***/
    private TransformManyBlock<string, string>? _firstBlock;
    private BroadcastBlock<string>? _broadcastRaw;
    private TransformBlock<string, DSMRTelegram>? _secondBlock;

    private ActionBlock<string>? _relayBlock;
    private ActionBlock<DSMRTelegram>? _lastBlock;
    /*** Dataflow ***/

    private IP1Reader? _reader;

    private readonly P1RelayServer _relayServer;
    private readonly IWatchdog _watchdog;
    private readonly int _intervalSeconds = 30;

    public SmartMeterMeasurementBase? LastMeasurement { get => Measurement; }

    private SmartMeterMeasurementBase? _measurement;
    protected SmartMeterMeasurementBase? Measurement
    {
        get
        {
            return _measurement;
        }
        set
        {
            _measurement = value;
            if (value != null)
                SmartMeterMeasurementAvailable.Invoke(this, new SmartMeterMeasurementAvailableEventArgs() { Measurement = value });
        }
    }

    public event EventHandler<SmartMeterMeasurementAvailableEventArgs> SmartMeterMeasurementAvailable = delegate { };

    public static void ConfigureServices(IServiceCollection services, AdapterInstance instance)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(instance);
        services.AddSingleton<P1RelayServer>();

        BackgroundServiceHelper.CreateAndStart<ISmartMeterAdapter, SmartMeter>(services, instance.Config);
    }


    public SmartMeter(ILogger<SmartMeter> logger, InstanceConfiguration config, P1RelayServer relayServer, IWatchdog watchdog)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(relayServer);
        ArgumentNullException.ThrowIfNull(watchdog);

        Logger = logger;
        _relayServer = relayServer;
        _watchdog = watchdog;
        Logger.LogInformation("SmartMeter({Cfg})", config.ToString().Replace(Environment.NewLine, " ", StringComparison.Ordinal));

        if (string.CompareOrdinal(config.Type.ToString(), "LAN") == 0)
        {
            _connectionType = ConnectionType.LAN;
            _host = config.Host;
            _port = config.Port;
        }
        else if (string.CompareOrdinal(config.Type.ToString(), "TTY") == 0)
        {
            _connectionType = ConnectionType.TTY;
            _usbPort = config.Device; // ie "/dev/sttyUSB1"
        }

        _buffer = new MessageBuffer();
    }

    protected override void Dispose(bool disposing)
    {
        Logger.LogTrace("Dispose({Disposing}) _disposed {Disposed}", disposing, Disposed);

        if (Disposed) return;

        if (disposing)
        {
            DisposeReader();
        }

        Logger.LogTrace("Dispose({Disposing}) done => _disposed {Disposed}", disposing, Disposed);

        base.Dispose(disposing);
    }

    private void DisposeReader()
    {
        _reader?.Dispose();
        _reader = null;
    }

    protected override async Task Start()
    {
        TransformManyBlock<string, string> firstBlock = CreateDataflow();
        _reader = CreateMeterReader(_connectionType, _host, _port, _usbPort);
        _reader.DataArrived += (sender, args) =>
        {
            firstBlock.Post(args.Data);
        };

        await _reader.StartAsync(CancellationToken).ConfigureAwait(false);

        _watchdog.Register(this, _intervalSeconds);
        await base.Start().ConfigureAwait(false);
    }

    protected override async void Stop()
    {
        base.Stop();
        _watchdog.Unregister(this);

        if (_reader is not null)
            await _reader.StopAsync(CancellationToken).ConfigureAwait(false);

        _firstBlock?.Complete();
        _broadcastRaw?.Complete();
        _secondBlock?.Complete();
        _lastBlock?.Complete();
        _relayBlock?.Complete();

        _firstBlock = null;
        _broadcastRaw = null;
        _secondBlock = null;
        _lastBlock = null;
        _relayBlock = null;
    }

    protected override DateTimeOffset GetNextOccurrence()
    {
        return DateTimeOffsetProvider.Now.AddMilliseconds(60 * 1000);
    }

    protected override Task DoBackgroundWork()
    {
        /* we are using the BckgroundWorker, since the watchdog doesnt support service */
        return Task.CompletedTask;
    }

    private TransformManyBlock<string, string> CreateDataflow()
    {
        _firstBlock = new TransformManyBlock<string, string>(input =>
        {
            _buffer.Add(input);
            var l = new List<string>();
            while (_buffer.TryTake(out string? msg) && msg != null)
            {
                Logger.LogDebug("first received complete message. passing it to transform... {Length}", msg.Length);
                l.Add(msg);
            }

            return l;
        });

        _broadcastRaw = new BroadcastBlock<string>(input => input);
        _firstBlock.LinkTo(_broadcastRaw);

        _secondBlock = new TransformBlock<string, DSMRTelegram>(input =>
        {
            Logger.LogDebug($"second received it and is going to transform it");
            var t = new DSMRTelegram(input);

            return t;
        });
        _broadcastRaw.LinkTo(_secondBlock);

        _lastBlock = new ActionBlock<DSMRTelegram>(x =>
        {
            Logger.LogDebug("read transformed message{NewLine}{Telegram}", Environment.NewLine, x);
            var m = new Reading.Measurement(x) { Received = DateTimeProvider.Now };

            Logger.LogDebug("Message {Measurement}", m);
            Measurement = m;
            _watchdog.Tick(this);
        });

        _secondBlock.LinkTo(_lastBlock);


        if (_relayServer != null)
        {
            Logger.LogDebug("Setup RelayServer");
            _relayBlock = new ActionBlock<string>(input =>
            {
                _relayServer.Relay(input);
            });
            _broadcastRaw.LinkTo(_relayBlock);
        }


        return _firstBlock;
    }

    static IP1Reader CreateMeterReader(ConnectionType connectionType, string? host, int? port, string? usbPort)
    {
        switch (connectionType)
        {
            case ConnectionType.LAN:
                if (string.IsNullOrWhiteSpace(host)) throw new ArgumentOutOfRangeException(nameof(connectionType), "Missing hostname for ConnectionType.LAN");
                if (!port.HasValue) throw new ArgumentOutOfRangeException(nameof(connectionType), "Missing port for ConnectionType.LAN");
                return new P1ReaderLAN(host, port.Value);
            case ConnectionType.TTY:
                if (string.IsNullOrWhiteSpace(usbPort)) throw new ArgumentOutOfRangeException(nameof(connectionType), "Missing usbport for ConnectionType.TTY");
                return new P1ReaderTTY(usbPort);
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionType), "Unhandled conntection type");
        }
    }
}