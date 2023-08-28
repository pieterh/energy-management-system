using System;
using System.IO.Ports;
using EMS.Library;
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
            var mock = SetupMock();
            var watchdockMock = mock.watchdog;
            var r = new P1ReaderTTY("/dev/usb", watchdockMock.Object);
            r.Disposed.Should().BeFalse();
            r.Dispose();
            r.Disposed.Should().BeTrue();
        }

        [Fact]
        [SuppressMessage("", "S3966")]
        public void CreateAndDoubleDispose()
        {
            var mock = SetupMock();
            var watchdockMock = mock.watchdog;

            var r = new P1ReaderTTY("/dev/usb", watchdockMock.Object);
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
            var watchdockMock = mock.watchdog;

            var r = new P1ReaderTTY("/dev/usb", watchdockMock.Object, serialPortFactoryMock.Object);
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
            var watchdockMock = mock.watchdog;

            var r = new P1ReaderTTY("/dev/usb", watchdockMock.Object, serialPortFactoryMock.Object);
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
            var (serialPortFactoryMock, serialPortMock, watchdockMock) = SetupMock();

            var r = new P1ReaderTTY("/dev/usb", watchdockMock.Object, serialPortFactoryMock.Object);
            var token = new CancellationToken();
            await r.StartAsync(token).ConfigureAwait(false);

            var receivedData = "Hello World!";
            serialPortMock.Setup<string>((x) => x.ReadExisting()).Returns(receivedData);

            DataArrivedEventArgs? dataArrived = null;
            r.DataArrived += (object? sender, DataArrivedEventArgs e) => { dataArrived = e; };

            // Manually simulate RaiseAsync by running it on a different thread.
            // Moq should support now the RaiseAsync, but somehow we get a null reference
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await Task.Run(() => serialPortMock.Raise((x) => x.DataReceived += null, serialPortMock.Object, null)).ConfigureAwait(false);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.


            dataArrived.Should().NotBeNull();
            Assert.NotNull(dataArrived);    // just get rid of warning
            dataArrived.Data.Should().NotBeNullOrWhiteSpace();
            dataArrived.Data.Should().BeEquivalentTo(receivedData);

            r.Dispose();
            r.Disposed.Should().BeTrue();
        }

        private static (Mock<ISerialPortFactory> serialPortFactory, Mock<ISerialPort> serialPort, Mock<IWatchdog> watchdog) SetupMock()
        {
            Mock<ISerialPortFactory> serialPortFactory = new Mock<ISerialPortFactory>();
            Mock<ISerialPort> serialPort = new Mock<ISerialPort>();
            Mock<IWatchdog> watchdog = new Mock<IWatchdog>();

            serialPortFactory.As<ISerialPortFactory>().Setup<ISerialPort>(s => s.CreateSerialPort(It.IsAny<string>())).Returns(serialPort.Object);

            return (serialPortFactory, serialPort, watchdog);
        }
    }
}