using System.Net;
using System.Net.Sockets;

namespace P1SmartMeter.Connection.Proxies
{
    /// <summary>
    /// This is just a subclass of SocketAsyncEventArgs and re-wires some of the functionality to the original classes
    /// </summary>
    [SuppressMessage("", "S3376")]
    internal class SocketAsyncEventArgsProxy : SocketAsyncEventArgs, ISocketAsyncEventArgs
    {
        ISocket? ISocketAsyncEventArgs.ConnectSocket { get { return (ISocket?)ConnectSocket; } }

        public new event EventHandler<ISocketAsyncEventArgs> Completed = (object? sender, ISocketAsyncEventArgs e) => { };

        internal SocketAsyncEventArgsProxy()
        {
            base.Completed += SocketAsyncEventArgsProxy_Completed;
        }

        private void SocketAsyncEventArgsProxy_Completed(object? sender, SocketAsyncEventArgs e)
        {
            Completed(sender, (ISocketAsyncEventArgs)e);
        }
    }
}
