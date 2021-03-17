using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using EMS.Library;
using System.Text;
using System.Collections.Generic;

namespace P1SmartMeter
{
    public class P1RelayServer : BackgroundWorker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly List<TcpClient> _clients = new List<TcpClient>();

        private TcpListener _listener;


        public P1RelayServer()
        {            
        }

        public override void Start()
        {
            base.Start();
            _listener = new TcpListener(IPAddress.Any, 8080);
            _listener.Start();
        }

        public override void Stop()
        {
            base.Stop();

            _listener?.Stop();
            _listener = null;
        }

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

        protected override void DoBackgroundWork()
        {
            try
            {
                TcpClient client = Task.Run(() => _listener.AcceptTcpClientAsync(), TokenSource.Token).GetAwaiter().GetResult();
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
        }
    }
}
