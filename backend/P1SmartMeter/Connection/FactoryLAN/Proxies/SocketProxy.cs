using System.Net;
using System.Net.Sockets;

namespace P1SmartMeter.Connection
{
    /// <summary>
    /// This is just a subclass of Socket and re-wires some of the functionality to the original classes
    /// </summary>
    internal class SocketProxy : Socket, ISocket
    {
        internal SocketProxy(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
        {
        }

        bool ISocket.ConnectAsync(ISocketAsyncEventArgs e)
        {
            return ConnectAsync((SocketAsyncEventArgs)e);
        }

        bool ISocket.ReceiveAsync(ISocketAsyncEventArgs e)
        {
            return ReceiveAsync((SocketAsyncEventArgs)e);
        }
    }
}
