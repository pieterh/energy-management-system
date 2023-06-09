using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using EMS.Library;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace P1SmartMeter
{
    public class P1RelayServer : BackgroundWorker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly List<TcpClient> _clients = new();
        private TcpListener? _listener;

        public P1RelayServer(IWatchdog watchdog):base(watchdog)
        {            
        }

        protected override Task Start()
        {           
            _listener = new TcpListener(IPAddress.Any, 8080);
            _listener.Start();
            return Task.CompletedTask;
        }

        protected override void Stop()
        {           
            _listener?.Stop();
            _listener = null;
        }

        [SuppressMessage("Code Analysis", "CA1031")]
        public void Relay(string data2relay)
        {
            var bytes = Encoding.ASCII.GetBytes(data2relay);
            List<TcpClient> clients;
            lock (_clients)
            {               
                clients = new List<TcpClient>(_clients);                
            }

            if (clients.Count == 0) return;

            Logger.Info($"Got bytes to relay to client #{_clients.Count}");
            clients.ForEach(x => {
                try
                {
                    if (x.Connected)
                    {
                        Logger.Info($"Sending to client");
                        x.GetStream().Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        lock (_clients)
                        {
                            Logger.Info($"Removing disconnected client");
                            _clients.Remove(x);
                            x.Close();
                            x.Dispose();
                        }
                    }         
                }catch(Exception e)
                {
                    Logger.Error($"Problem relaying to client. {e.Message}");
                }               
            });
        }

        [SuppressMessage("Code Analysis", "CA1031")]
        protected override Task DoBackgroundWork()
        {
            try
            {
                TcpClient client = Task.Run(() => _listener?.AcceptTcpClientAsync(), CancellationToken).GetAwaiter().GetResult();
                Logger.Info($"Client connected!");
                client.SendBufferSize = 2048;
                lock (_clients)
                {
                    _clients.Add(client);
                    Logger.Info($"added client #{_clients.Count}");                    
                }
            } catch (OperationCanceledException) { /* We expecting the cancelation exception and don't need to act on it */
            } catch (Exception e) 
            {
                Logger.Error("Exception: " + e.Message);
            }
            return Task.CompletedTask;
        }
    }
}
