using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EMS.Library;
using EMS.Library.Adapter.SmartMeterAdapter;
using EMS.Library.Configuration;
using EMS.Library.TestableDateTime;

using P1SmartMeter.Connection;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter
{
    public class SmartMeter : Microsoft.Extensions.Hosting.BackgroundService, ISmartMeterAdapter
    {
        private readonly ILogger Logger;
        private bool _disposed;

        private enum ConnectionType { LAN, TTY };
        private readonly ConnectionType _connectionType;
        private readonly string _host;
        private readonly int _port;

        private readonly string _usbPort;

        private IP1Reader _reader;
        private MessageBuffer _buffer;
        private readonly P1RelayServer _relayServer;

        public SmartMeterMeasurementBase LastMeasurement { get => Measurement; }

        private SmartMeterMeasurement _measurement;
        protected SmartMeterMeasurement Measurement
        {
            get
            {
                return _measurement;
            }
            set
            {
                _measurement = value;
                SmartMeterMeasurementAvailable?.Invoke(this, new SmartMeterMeasurementAvailableEventArgs() { Measurement = value });
            }
        }

        public event EventHandler<SmartMeterMeasurementAvailableEventArgs> SmartMeterMeasurementAvailable;

        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services, Instance instance)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(instance);
            services.AddSingleton<P1RelayServer>();

            BackgroundServiceHelper.CreateAndStart<ISmartMeterAdapter, SmartMeter>(services, instance.Config);
        }


        public SmartMeter(ILogger<SmartMeter> logger, Config config, P1RelayServer relayServer)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(relayServer);

            Logger = logger;
            _relayServer = relayServer;
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
        }

        public override void Dispose()
        {
            Grind(true);
            base.Dispose();
            GC.SuppressFinalize(this);  // Suppress finalization.
        }

        protected void Grind(bool disposing)
        {
            Logger.LogTrace("Dispose({Disposing}) _disposed {Disposed}", disposing, _disposed);

            if (_disposed) return;

            if (disposing)
            {
                DisposeReader();
            }

            _disposed = true;
            Logger.LogTrace("Dispose({Disposing}) done => _disposed {Disposed}", disposing, _disposed);
        }

        private void DisposeReader()
        {
            _reader?.Dispose();
            _reader = null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("P1SmartMeter Starting");
            try
            {
                TransformManyBlock<string, string> _firstBlock;
                BroadcastBlock<string> _broadcastRaw;
                TransformBlock<string, DSMRTelegram> _secondBlock;

                ActionBlock<string> _relayBlock;
                ActionBlock<DSMRTelegram> _lastBlock;

                _buffer = new MessageBuffer();
                _firstBlock = new TransformManyBlock<string, string>(input =>
                {
                    _buffer.Add(input);
                    var l = new List<string>();
                    while (_buffer.TryTake(out string msg))
                    {
                        Logger.LogDebug("first received complete message. passing it to transform... {Length}", msg.Length);
                        l.Add(msg);
                    }

                    return l;
                });

                _broadcastRaw = new BroadcastBlock<string>(input => input);

                _secondBlock = new TransformBlock<string, DSMRTelegram>(input =>
                {
                    Logger.LogDebug($"second received it and is going to transform it");
                    var t = new DSMRTelegram(input);

                    return t;
                });

                _lastBlock = new ActionBlock<DSMRTelegram>(x =>
                {
                    Logger.LogDebug("read transformed message{NewLine}{Telegram}", Environment.NewLine, x);
                    var m = new Reading.Measurement(x) { Received = DateTimeProvider.Now };

                    Logger.LogDebug("Message {Measurement}", m);
                    Measurement = m;
                });

                _firstBlock.LinkTo(_broadcastRaw);

                _broadcastRaw.LinkTo(_secondBlock);

                if (_relayServer != null)
                {
                    _relayBlock = new ActionBlock<string>(input =>
                    {
                        _relayServer.Relay(input);
                    });
                    _broadcastRaw.LinkTo(_relayBlock);
                }

                _secondBlock.LinkTo(_lastBlock);

                _reader = CreateMeterReader();
                _reader.DataArrived += (sender, args) =>
                {
                    _firstBlock.Post(args.Data);
                };

                try
                {
                    await _reader.StartAsync(stoppingToken).ConfigureAwait(false);
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await _reader.StopAsync(stoppingToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                Logger.LogInformation("Canceled");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled exceptio");
            }
        }

        internal IP1Reader CreateMeterReader()
        {
            switch (_connectionType)
            {
                case ConnectionType.LAN:
                    return new P1ReaderLAN(_host, _port);
                case ConnectionType.TTY:
                    return new P1ReaderTTY(_usbPort);
            }
            return null;
        }
    }
}