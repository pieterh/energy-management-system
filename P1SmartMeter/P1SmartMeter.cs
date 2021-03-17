using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks.Dataflow;
using EMS.Library;
using Newtonsoft.Json.Linq;
using P1SmartMeter.Connection;
using P1SmartMeter.Reading;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter
{
    public class P1SmartMeter : BackgroundWorker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private enum ConnectionType { LAN, TTY };
        private ConnectionType _connectionType;
        private string _host;
        private int _port;

        private readonly string _usbPort;

        private IP1Interface _reader = null;
        private MessageBuffer _buffer = null;
        private P1RelayServer _relayServer = null;

        TransformManyBlock<string, string> _firstBlock;
        BroadcastBlock<string> _broadcastRaw;
        TransformBlock<string, DSMRTelegram> _secondBlock;

        ActionBlock<string> _relayBlock;
        ActionBlock<DSMRTelegram> _lastBlock;

        private Measurement _measurement;

        public Measurement Measurement { get => _measurement; set => _measurement = value; }

        public P1SmartMeter(JObject config)
        {
            dynamic cfg = config;
            Logger.Info($"P1SmartMeter({config.ToString().Replace(Environment.NewLine, " ")})");

            if (string.CompareOrdinal(cfg.type.ToString(), "LAN") == 0){
                _connectionType = ConnectionType.LAN;
                _host = cfg.host;
                _port = cfg.port;
            }
            else if (string.CompareOrdinal(cfg.type.ToString(), "TTY") == 0){
                _connectionType = ConnectionType.TTY;
                _usbPort = cfg.port;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeReader();
        }

        private void DisposeReader()
        {
            if (_reader == null) return;

            try
            {
                _reader.Dispose();
            }
            finally
            {
                _reader = null;
            }

        }

        public override void Start()
        {
            Logger.Info($"P1SmartMeter Starting");
            base.Start();

            _relayServer = new P1RelayServer();
            _relayServer.Start();

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
                _measurement = new Measurement(x);
                Logger.Info($"Message {_measurement}");                
            });

            _relayBlock = new ActionBlock<string>(input =>
            {
                _relayServer.Relay(input);
            });
            
            _firstBlock.LinkTo(_broadcastRaw);

            _broadcastRaw.LinkTo(_secondBlock);
            _broadcastRaw.LinkTo(_relayBlock);

            _secondBlock.LinkTo(_lastBlock);

            _reader = CreateMeterReader();
            _reader.DataArrived += (sender, args) =>
            {
                _firstBlock.Post(args.Data);
            };

            _reader.Start();
        }

        public override void Stop()
        {
            base.Stop();

            _relayServer.Stop();
            _relayServer.Dispose();
            _relayServer = null;

            _reader.Stop();
            _reader.Dispose();
            _reader = null;
        }

        protected override void DoBackgroundWork()
        {
            // nothing to do here
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