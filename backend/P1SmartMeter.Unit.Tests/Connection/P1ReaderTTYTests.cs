using System;
using System.IO.Ports;
using Moq;
using P1SmartMeter.Connection;
using P1SmartMeter.Connection.Factories;
using P1SmartMeter.Connection.Proxies;
using static P1ReaderUnitTests.P1ReaderLANTests;

namespace P1ReaderUnitTests
{
    public class P1ReaderTTYTests
    {
        [Fact]
        public void CreateAndDispose()
        {
            var r = new P1ReaderTTY("/dev/usb");
            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();
        }

        [Fact]
        [SuppressMessage("", "S3966")]
        public void CreateAndDoubleDispose()
        {
            var r = new P1ReaderTTY("/dev/usb");
            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();
            r.Dispose();
            r.Disposed.Should().BeTrue();
        }

        [Fact]
        public async Task CreateStartAndDispose()
        {
            var mock = SetupMock();
            var serialPortFactoryMock = mock.serialPortFactory;
            var serialPortMock = mock.serialPort;

            var r = new P1ReaderTTY("/dev/usb", serialPortFactoryMock.Object);
            var token = new CancellationToken();
            await r.StartAsync(token).ConfigureAwait(false);

            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();

            serialPortMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task StartAsyncAndStopAsync()
        {
            var mock = SetupMock();
            var serialPortFactoryMock = mock.serialPortFactory;
            var serialPortMock = mock.serialPort;

            var r = new P1ReaderTTY("/dev/usb", serialPortFactoryMock.Object);
            var token = new CancellationToken();
            await r.StartAsync(token).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
            await r.StopAsync(token).ConfigureAwait(false);

            serialPortMock.Verify(x => x.Dispose(), Times.Once);

            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();            
        }


        [Fact]
        public async Task StartAsyncAndReceiveData()
        {
            var mock = SetupMock();
            var serialPortFactoryMock = mock.serialPortFactory;
            var serialPortMock = mock.serialPort;

            var r = new P1ReaderTTY("/dev/usb", serialPortFactoryMock.Object);
            var token = new CancellationToken();
            await r.StartAsync(token).ConfigureAwait(false);

            var receivedData = "Hello World!";
            serialPortMock.Setup<string>((x) => x.ReadExisting()).Returns(receivedData);

            DataArrivedEventArgs? dataArrived = null;
            r.DataArrived += (object? sender, DataArrivedEventArgs e) => { dataArrived = e; };

            serialPortMock.Raise((x) => x.DataReceived += null, serialPortMock.Object, null);   /* no need to pass eventargs since there is nothing we can do with it */

            dataArrived.Should().NotBeNull();
            Assert.NotNull(dataArrived);    // just get rid of warning
            dataArrived.Data.Should().NotBeNullOrWhiteSpace();
            dataArrived.Data.Should().BeEquivalentTo(receivedData);

            r.Dispose();
            r.Disposed.Should().BeTrue();
        }

        private static (Mock<ISerialPortFactory> serialPortFactory, Mock<ISerialPort> serialPort) SetupMock()
        {
            Mock<ISerialPortFactory> serialPortFactory = new Mock<ISerialPortFactory>();
            Mock<ISerialPort> serialPort = new Mock<ISerialPort>();

            serialPortFactory.As<ISerialPortFactory>().Setup<ISerialPort>(s => s.CreateSerialPort(It.IsAny<string>())).Returns(serialPort.Object);
            return (serialPortFactory, serialPort);
        }
    }
}

