using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EMS.Library;
using EMS.Library.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using P1SmartMeter.Connection;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter
{
    public class P1SmartMeter : BackgroundService, ISmartMeter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _disposed = false;

        private enum ConnectionType { LAN, TTY };
        private readonly ConnectionType _connectionType;
        private readonly string _host;
        private readonly int _port;

        private readonly string _usbPort;

        private IP1Interface _reader = null;
        private MessageBuffer _buffer = null;
        private readonly P1RelayServer _relayServer = null;

        private Reading.Measurement _measurement;
        public Reading.Measurement Measurement
        {
            get => _measurement;
            protected set
            {
                _measurement = value;
                MeasurementAvailable?.Invoke(this, new ISmartMeter.MeasurementAvailableEventArgs() { Measurement = value });
            }
        }

        public event EventHandler<ISmartMeter.MeasurementAvailableEventArgs> MeasurementAvailable;

        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services, Instance instance)
        {
            services.AddSingleton<P1RelayServer>();

            services.AddSingleton(typeof(ISmartMeter), x =>
            {
                var s = ActivatorUtilities.CreateInstance(x, typeof(P1SmartMeter), instance.Config);
                Logger.Info($"Instance [{instance.Name}], created");
                return s;
            });
            services.AddSingleton<IHostedService>(x =>
            {
                var s = x.GetService(typeof(ISmartMeter)) as IHostedService;
                return s;
            });
        }


        public P1SmartMeter(Config config, P1RelayServer relayServer)
        {
            _relayServer = relayServer;

            Logger.Info($"P1SmartMeter({config.ToString().Replace(Environment.NewLine, " ")})");

            if (string.CompareOrdinal(config.Type.ToString(), "LAN") == 0)
            {
                _connectionType = ConnectionType.LAN;
                _host = config.Host;
                _port = config.Port;
            }
            else if (string.CompareOrdinal(config.Type.ToString(), "TTY") == 0)
            {
                _connectionType = ConnectionType.TTY;
                _usbPort = "/dev/sttyUSB1"; //config.Port;
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
            Logger.Trace($"Dispose({disposing}) _disposed {_disposed}");

            if (_disposed) return;

            if (disposing)
            {
                DisposeReader();
            }

            _disposed = true;
            Logger.Trace($"Dispose({disposing}) done => _disposed {_disposed}");
        }

        private void DisposeReader()
        {
            _reader?.Stop();
            _reader?.Dispose();
            _reader = null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info($"P1SmartMeter Starting");
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
                    Logger.Debug($"first received complete message. passing it to transform... {msg.Length}");
                    l.Add(msg);
                }

                return l;
            });

            _broadcastRaw = new BroadcastBlock<string>(input => input);

            _secondBlock = new TransformBlock<string, DSMRTelegram>(input =>
            {
                Logger.Debug($"second received it and is going to transform it");
                var t = new DSMRTelegram(input);

                return t;
            });

            _lastBlock = new ActionBlock<DSMRTelegram>(x =>
            {
                Logger.Debug($"read transformed message{Environment.NewLine}{x}");
                var m = new Reading.Measurement(x) { Received = DateTime.Now };

                Logger.Debug($"Message {m}");
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
                _reader.Start();
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            finally
            {
                _reader.Stop();
                Logger.Info($"Canceled");
            }
        }

        public IP1Interface CreateMeterReader()
        {
            switch (_connectionType)
            {
                case ConnectionType.LAN:
                    return new LANReader(_host, _port);
                case ConnectionType.TTY:
                    return new TTYReader(_usbPort);
            }
            return null;
        }
    }
}