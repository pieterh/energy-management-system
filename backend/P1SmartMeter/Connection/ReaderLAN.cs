using System;
using System.Net.Sockets;
using System.Text;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public class ReaderLAN : Reader                                   //NOSONAR
    {
        private readonly string _host;
        private readonly int _port;

        public ReaderLAN(string host, int port)
        {
            _host = host;
            _port = port;
        }

        protected override void Run()
        {
            Logger.Info($"BackgroundTask run");

            using (var tcpClient = new TcpClient(_host, _port))
            {
                Logger.Info($"BackgroundTask connected");
                var bufje = new byte[4096];

                tcpClient.ReceiveBufferSize = 4096;
                tcpClient.ReceiveTimeout = 30000;
                using var s = tcpClient.GetStream();

                while (!StopRequested(250))
                {
                    Logger.Trace($"BackgroundTask reading!");
                    var nrCharsRead = s.Read(bufje, 0, bufje.Length);

                    Logger.Debug($"BackgroundTask read {nrCharsRead} bytes...");
                    var tmp = new byte[nrCharsRead];
                    Buffer.BlockCopy(bufje, 0, tmp, 0, nrCharsRead);

                    OnDataArrived(new DataArrivedEventArgs() { Data = Encoding.ASCII.GetString(tmp) });
                }

                s.Close();
            }

            if (_tokenSource.Token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }
    }

}
