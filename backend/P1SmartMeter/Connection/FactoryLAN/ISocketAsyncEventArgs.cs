using System.Net;
using System.Net.Sockets;

namespace P1SmartMeter.Connection
{
    internal interface ISocketAsyncEventArgs : IDisposable
    {
        void SetBuffer(byte[]? buffer, int offset, int count);
        event EventHandler<ISocketAsyncEventArgs> Completed;
        EndPoint? RemoteEndPoint { get; set; }
        SocketAsyncOperation LastOperation { get; }
        ISocket? ConnectSocket { get; }
        SocketError SocketError { get; }
        int BytesTransferred { get; }
        Memory<byte> MemoryBuffer { get; }
    }
}
