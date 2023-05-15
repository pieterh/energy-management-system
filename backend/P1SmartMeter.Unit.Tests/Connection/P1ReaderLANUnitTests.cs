using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Moq;
using Moq.Protected;
using P1SmartMeter.Connection;
using Xunit.Abstractions;
using static P1ReaderUnitTests.P1ReaderLANTests;

namespace P1ReaderUnitTests
{
    public class P1ReaderLANTests
    {
        [Fact]
        public void CreateAndDispose()
        {
            var r = new P1ReaderLAN("localhost", 8080);
            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();
        }

        [Fact]
        [SuppressMessage("", "S3966")]
        public void CreateAndDoubleDispose()
        {
            var r = new P1ReaderLAN("localhost", 8080);
            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();
            r.Dispose();
            r.Disposed.Should().BeTrue();
        }

        [Fact]
        public void CreateConnectAndDispose()
        {
            var m = SetupMocks();
            var socketFactory = m.factory;
            var socketMock = m.socket;
            var socketAsyncEventArgsMock = m.socketAsyncEventArgs;

            // ConnectAsync will indicate no data pending
            socketMock.Setup<bool>(s => s.ConnectAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(true);

            var r = new P1ReaderLAN("127.0.0.1", 8080, socketFactory.Object);

            r.Disposed.Should().BeFalse();
            r.Connect().Should().BeTrue();
            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();

            // and verify if the factory created objects are properly disposed of...
            socketMock.Verify(x => x.Dispose(), Times.Once);
            socketAsyncEventArgsMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task StartAsyncAndStopAsync()
        {
            var m = SetupMocks();
            var socketFactory = m.factory;
            var socketMock = m.socket;
            var socketAsyncEventArgsMock = m.socketAsyncEventArgs;

            // ConnectAsync will indicate no data pending
            socketMock.Setup<bool>(s => s.ConnectAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(true);

            var r = new P1ReaderLAN("127.0.0.1", 8080, socketFactory.Object);
            var token = new CancellationToken();
            await r.StartAsync(token).ConfigureAwait(false);

            // wait a bit until the service is connecting
            for (int i = 0; i < 50 && r.Status != ConnectionStatus.Connecting; i++)
                Thread.Sleep(100);
            r.Status.Should().Be(ConnectionStatus.Connecting);

            await r.StopAsync(token).ConfigureAwait(false);

            // after the stop, the connection status should be disconnected
            r.Status.Should().Be(ConnectionStatus.Disconnected);

            // and verify if the factory created objects are properly disposed of...
            socketMock.Verify(x => x.Dispose(), Times.AtLeastOnce);
            socketAsyncEventArgsMock.Verify(x => x.Dispose(), Times.AtLeastOnce);

            r.Dispose();
            r.Disposed.Should().BeTrue();

            socketAsyncEventArgsMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void DoesConnectAndNoImmediateConnectedEvent()
        {
            var m = SetupMocks();
            var socketFactory = m.factory;
            var socketMock = m.socket;
            var socketAsyncEventArgsMock = m.socketAsyncEventArgs;

            // ConnectAsync and ReceiveAsync will indicate no data pending => expecting later an event
            socketMock.Setup<bool>(s => s.ConnectAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(true);
            socketMock.Setup<bool>(s => s.ReceiveAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(true);

            using var r = new P1ReaderLAN("127.0.0.1", 8080, socketFactory.Object);
            DataArrivedEventArgs? lastEvent = null;
            r.DataArrived += (object? sender, DataArrivedEventArgs e) =>
            {
                lastEvent = e;
            };

            r.Status.Should().Be(ConnectionStatus.Disconnected);
            r.Connect().Should().BeTrue();
            r.Status.Should().Be(ConnectionStatus.Connecting);

            // prepare for event raising indicating that the connection compleet
            socketAsyncEventArgsMock.Setup(s => s.LastOperation).Returns(SocketAsyncOperation.Connect);
            socketAsyncEventArgsMock.Setup(s => s.SocketError).Returns(SocketError.Success);
            socketMock.Setup<bool>(s => s.Connected).Returns(true);
            socketAsyncEventArgsMock.Raise((x) => { x.Completed += null; }, (EventArgs)socketAsyncEventArgsMock.Object);
            r.Status.Should().Be(ConnectionStatus.Connected);

            lastEvent.Should().BeNull("There was no data pending");
        }

        [Fact]
        public void DoesConnectAndDataArrived()
        {
            var m = SetupMocks();
            var socketFactory = m.factory;
            var socketMock = m.socket;
            var socketAsyncEventArgsMock = m.socketAsyncEventArgs;

            // ConnectAsync and ReceiveAsync will indicate no data pending => expecting later an event
            socketMock.Setup<bool>(s => s.ConnectAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(false);
            socketMock.Setup<bool>(s => s.ReceiveAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(true);

            using var r = new P1ReaderLAN("127.0.0.1", 8080, socketFactory.Object);
            DataArrivedEventArgs? lastEvent = null;
            r.DataArrived += (object? sender, DataArrivedEventArgs e) =>
            {
                lastEvent = e;
            };

            r.Status.Should().Be(ConnectionStatus.Disconnected);
            r.Connect().Should().BeTrue();

            // prepare for event raising indicating that the connection compleet
            socketAsyncEventArgsMock.Setup(s => s.LastOperation).Returns(SocketAsyncOperation.Connect);
            socketAsyncEventArgsMock.Setup(s => s.SocketError).Returns(SocketError.Success);
            socketMock.Setup<bool>(s => s.Connected).Returns(true);

            socketAsyncEventArgsMock.Raise((x) => { x.Completed += null; }, (EventArgs)socketAsyncEventArgsMock.Object);
            r.Status.Should().Be(ConnectionStatus.Connected);

            // prepare for event raising indicating that there is data
            socketAsyncEventArgsMock.Setup(s => s.LastOperation).Returns(SocketAsyncOperation.Receive);
            socketAsyncEventArgsMock.Setup(s => s.SocketError).Returns(SocketError.Success);

            var stringReceived = "Hello World!";

            var bytes = Encoding.ASCII.GetBytes(stringReceived);

            bytes.AsSpan().TryCopyTo(socketAsyncEventArgsMock.Object.MemoryBuffer.Span);
            socketAsyncEventArgsMock.Setup(s => s.BytesTransferred).Returns(bytes.Length);

            socketAsyncEventArgsMock.Raise((x) => { x.Completed += null; }, (EventArgs)socketAsyncEventArgsMock.Object);
            lastEvent.Should().NotBeNull();
            Assert.NotNull(lastEvent); // prevent null warning
            lastEvent.Data.Should().NotBeNullOrWhiteSpace();
            lastEvent.Data.Should().BeEquivalentTo(stringReceived);
        }

        [Fact]
        public void DoesConnectAndDataArrivedMultiple()
        {
            var m = SetupMocks();
            var socketFactory = m.factory;
            var socketMock = m.socket;
            var socketAsyncEventArgsMock = m.socketAsyncEventArgs;

            // ConnectAsync and ReceiveAsync will indicate no data pending => expecting later an event
            socketMock.Setup<bool>(s => s.ConnectAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(false);
            socketMock.Setup<bool>(s => s.ReceiveAsync(It.IsAny<ISocketAsyncEventArgs>())).Returns(true);

            using var r = new P1ReaderLAN("127.0.0.1", 8080, socketFactory.Object);
            DataArrivedEventArgs? lastEvent = null;
            r.DataArrived += (object? sender, DataArrivedEventArgs e) =>
            {
                lastEvent = e;
            };

            r.Status.Should().Be(ConnectionStatus.Disconnected);
            r.Connect().Should().BeTrue();

            // prepare for event raising indicating that the connection compleet
            socketAsyncEventArgsMock.Setup(s => s.LastOperation).Returns(SocketAsyncOperation.Connect);
            socketAsyncEventArgsMock.Setup(s => s.SocketError).Returns(SocketError.Success);
            socketMock.Setup<bool>(s => s.Connected).Returns(true);

            socketAsyncEventArgsMock.Raise((x) => { x.Completed += null; }, (EventArgs)socketAsyncEventArgsMock.Object);
            r.Status.Should().Be(ConnectionStatus.Connected);

            // prepare for event raising indicating that there is data
            socketAsyncEventArgsMock.Setup(s => s.LastOperation).Returns(SocketAsyncOperation.Receive);
            socketAsyncEventArgsMock.Setup(s => s.SocketError).Returns(SocketError.Success);

            var stringReceived = "Hello World!";

            var bytes = Encoding.ASCII.GetBytes(stringReceived);

            bytes.AsSpan().TryCopyTo(socketAsyncEventArgsMock.Object.MemoryBuffer.Span);
            socketAsyncEventArgsMock.Setup(s => s.BytesTransferred).Returns(bytes.Length);

            // after the event, immediatly one more set of data is available
            socketMock.SetupSequence<bool>(s => s.ReceiveAsync(It.IsAny<ISocketAsyncEventArgs>()))
                .Returns(false)
                .Returns(true);

            socketAsyncEventArgsMock.Raise((x) => { x.Completed += null; }, (EventArgs)socketAsyncEventArgsMock.Object);
            lastEvent.Should().NotBeNull();
            Assert.NotNull(lastEvent); // prevent null warning
            lastEvent.Data.Should().NotBeNullOrWhiteSpace();
            lastEvent.Data.Should().BeEquivalentTo(stringReceived);

            // once directly after, setup connection
            // and then twice with data
            socketMock.Verify(x => x.ReceiveAsync(It.IsAny<ISocketAsyncEventArgs>()), Times.Exactly(3));
        }

        [Fact]
        public void HandlesSocketExceptionWhenConnectingTimedOut()
        {
            var m = SetupMocks();
            var socketFactory = m.factory;
            var socketMock = m.socket;

            socketMock.Setup<bool>(s => s.ConnectAsync(It.IsAny<ISocketAsyncEventArgs>())).Throws<SocketException>(() => { throw new SocketException((int)SocketError.TimedOut); });

            using var r = new P1ReaderLAN("127.0.0.1", 8080, socketFactory.Object);
            r.Connect().Should().BeFalse();
        }

        [Fact]
        public void HandlesSocketExceptionWhenConnectingHostUnreachable()
        {
            var m = SetupMocks();
            var socketFactory = m.factory;
            var socketMock = m.socket;

            socketMock.Setup<bool>(s => s.ConnectAsync(It.IsAny<ISocketAsyncEventArgs>())).Throws<SocketException>(() => { throw new SocketException((int)SocketError.HostUnreachable); });

            using var r = new P1ReaderLAN("127.0.0.1", 8080, socketFactory.Object);
            r.Connect().Should().BeFalse();
        }

        /// <summary>
        /// Setup all basics for the mocks
        /// </summary>
        /// <returns></returns>
        private static (Mock<ISocketFactory> factory, Mock<ISocket> socket, Mock<ISocketAsyncEventArgs> socketAsyncEventArgs) SetupMocks()
        {
            Mock<ISocketFactory> socketFactory = new Mock<ISocketFactory>();
            Mock<ISocket> socketMock = new Mock<SocketMock>().As<ISocket>();
            socketMock.Setup(s => s.LocalEndPoint).Returns(() => new IPEndPoint(2130706433, 24000));

            Mock<ISocketAsyncEventArgs> socketAsyncEventArgsMock = new Mock<SocketAsyncEventArgsMock>().As<ISocketAsyncEventArgs>();
            socketAsyncEventArgsMock.Setup(s => s.ConnectSocket).Returns(socketMock.Object);
            socketAsyncEventArgsMock.Setup(s => s.SetBuffer(It.IsAny<byte[]?>(), It.IsAny<int>(), It.IsAny<int>())).CallBase();
            socketAsyncEventArgsMock.Setup(s => s.MemoryBuffer).CallBase();

            socketFactory.As<ISocketFactory>().Setup<ISocket>(s => s.CreateSocket(SocketType.Stream, ProtocolType.Tcp)).Returns(socketMock.Object);
            socketFactory.As<ISocketFactory>().Setup<ISocketAsyncEventArgs>(s => s.CreateSocketAsyncEventArgs()).Returns(socketAsyncEventArgsMock.Object);
            return (factory: socketFactory, socket: socketMock, socketAsyncEventArgs: socketAsyncEventArgsMock);
        }

        internal class P1ReaderTester : P1Reader
        {
            public void SomeData(string data)
            {
                var dae = new DataArrivedEventArgs(data);
                this.OnDataArrived(dae);
            }

            protected override Task Start()
            {
                throw new NotImplementedException();
            }

            protected override void Stop()
            {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("", "S3881")]
        internal class SocketMock : ISocket
        {
            public virtual bool IsDisposed { get; internal set; }
            public virtual bool Connected { get { return true; } }
            public virtual EndPoint? LocalEndPoint { get { return null; } }

            public SocketMock() { /* mock */ }

            public virtual void Close() { /* mock */ }
            public virtual bool ConnectAsync(SocketAsyncEventArgs e) { return false; }
            public virtual bool ReceiveAsync(SocketAsyncEventArgs e) { return false; }
            public virtual void Disconnect(bool reuseSocket) { /* mock */ }
            protected virtual void Dispose(bool disposing)
            {
                IsDisposed = true;
            }

            public virtual void Dispose() { /* mock */ }

            public bool ConnectAsync(ISocketAsyncEventArgs e)
            {
                throw new NotImplementedException();
            }

            public bool ReceiveAsync(ISocketAsyncEventArgs e)
            {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("", "S3881")]
        [SuppressMessage("", "S3376")]
        internal class SocketAsyncEventArgsMock : EventArgs, ISocketAsyncEventArgs
        {
            public EndPoint? RemoteEndPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public SocketAsyncOperation LastOperation => throw new NotImplementedException();

            public ISocket? ConnectSocket => throw new NotImplementedException();

            public SocketError SocketError => throw new NotImplementedException();

            public int BytesTransferred => throw new NotImplementedException();

            public event EventHandler<ISocketAsyncEventArgs> Completed = (Object? sender, ISocketAsyncEventArgs e) => { };

            public void Dispose()
            {
                /* dummy mock object... nothing to dispose */
            }

            public Memory<byte> MemoryBuffer { get; internal set; }

            public void SetBuffer(byte[]? buffer, int offset, int count)
            {
                MemoryBuffer = new Memory<byte>(buffer, offset, count);
            }
        }
    }
}
