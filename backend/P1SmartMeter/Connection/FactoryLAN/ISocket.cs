using System.Net;

namespace P1SmartMeter.Connection
{
    internal interface ISocket : IDisposable
    {
        bool Connected { get; }
        EndPoint? LocalEndPoint { get; }
        void Close();
        /// <summary>
        /// sum,
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns>
        /// true if the I/O operation is pending. The Completed event on the e parameter will be raised upon completion of the operation.
        /// false if the I/O operation completed synchronously.In this case, The Completed event on the e parameter will not be raised and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool ConnectAsync(ISocketAsyncEventArgs e);
        /// <summary>
        /// true if the I/O operation is pending. The Completed event on the e parameter will be raised upon completion of the operation.
        /// false if the I/O operation completed synchronously.In this case, The Completed event on the e parameter will not be raised and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool ReceiveAsync(ISocketAsyncEventArgs e);
        void Disconnect(bool reuseSocket);
    }
}
