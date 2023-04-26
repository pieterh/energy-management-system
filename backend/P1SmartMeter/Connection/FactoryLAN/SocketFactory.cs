using System.Net.Sockets;
using P1SmartMeter.Connection.Proxies;

namespace P1SmartMeter.Connection
{
    internal interface ISocketFactory
    {
        ISocket CreateSocket(SocketType socketType, ProtocolType protocolType);
        ISocketAsyncEventArgs CreateSocketAsyncEventArgs();
    }

    internal class SocketFactory : ISocketFactory
    {
        public ISocket CreateSocket(SocketType socketType, ProtocolType protocolType)
        {
            return new SocketProxy(socketType, protocolType);
        }

        public ISocketAsyncEventArgs CreateSocketAsyncEventArgs()
        {
            return new SocketAsyncEventArgsProxy();
        }
    }
}

