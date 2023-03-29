using Xunit;
using FluentAssertions;

namespace P1SmartMeter.CRC16Tests
{
    public class ValidateArguments
    {
        [Fact(DisplayName = "ComputeChecksumBytes(byte[] bytes) will throw ArgumentNullException")]
        public void ComputeChecksumBytesNull()
        {
            var c = new CRC16();
            Action act = () => c.ComputeChecksumBytes(null);
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ComputeChecksum(byte[] bytes) will throw ArgumentNullException")]
        public void ComputeChecksumNull1()
        {
            var c = new CRC16();
            Action act = () => c.ComputeChecksum(null);
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ComputeChecksum(byte[] bytes, int length) will throw ArgumentNullException")]
        public void ComputeChecksumNull2()
        {
            var c = new CRC16();
            Action act = () => c.ComputeChecksum(null, -1);
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ComputeChecksum(byte[] bytes, int length) will throw ArgumentOutOfRangeException")]
        public void ComputeChecksumLength()
        {
            var c = new CRC16();
            Action act = () => c.ComputeChecksum(new byte[] { 0x00 }, -1);
            _ = act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = "ComputeChecksumAsString(byte[] bytes, int length) will throw ArgumentNullException")]
        public void ComputeChecksumAsStringNull()
        {
            var c = new CRC16();
            Action act = () => c.ComputeChecksumAsString(null, 1);
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ComputeChecksumAsString(byte[] bytes, int length) will throw ArgumentOutOfRangeException")]
        public void ComputeChecksumAsStringLength()
        {
            var c = new CRC16();
            Action act = () => c.ComputeChecksumAsString(new byte[] { 0x00 }, -1);
            _ = act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = "ValidateChecksum(byte[] bytes, string checksum) will throw ArgumentNullException")]
        public void ValidateChecksumNull1()
        {
            var c = new CRC16();
            Action act = () => c.ValidateChecksum(null, "123");
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ValidateChecksum(byte[] bytes, string checksum) will throw ArgumentNullException")]
        public void ValidateChecksumNull2()
        {
            var c = new CRC16();
            Action act = () => c.ValidateChecksum(new byte[] { 0x00 }, null);
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ValidateChecksum(byte[] bytes, ushort checksum) will throw ArgumentNullException")]
        public void ValidateChecksumNull3()
        {
            var c = new CRC16();
            Action act = () => c.ValidateChecksum(null, 0x1234);
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ValidateChecksum(byte[] bytes, int length, string checksum) will throw ArgumentOutOfRangeException")]
        public void ValidateChecksumNull4()
        {
            var c = new CRC16();
            Action act = () => c.ValidateChecksum(new byte[] { 0x00 }, -1, "123");
            _ = act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = "ValidateChecksum(byte[] bytes, int length, ushort checksum) will throw ArgumentOutOfRangeException")]
        public void ValidateChecksumNull5()
        {
            var c = new CRC16();
            Action act = () => c.ValidateChecksum(new byte[] { 0x00 }, -1, 0x1234);
            _ = act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    public class Computation
    {
        [Theory(DisplayName = "Validates byte array with a given crc16 as a short value")]
        [InlineData(new byte[] { 0x00 }, 0x0000, true)]
        [InlineData(new byte[] { 0x00, 0x00 }, 0x0000, true)]
        [InlineData(new byte[] { 0xAA, 0xBB }, 0xD33E, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0xbb3d, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0x1111, false)]
        public void CheckCRC16Short(byte[] items, ushort crc16, bool isValid)
        {
            var c = new CRC16();

            Assert.Equal(c.ValidateChecksum(items, crc16), isValid);
        }

        [Theory(DisplayName = "Validates byte array with a given crc16 as a string value")]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, "BB3D", true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, "1111", false)]
        public void CheckCRC16String(byte[] items, string crc16, bool isValid)
        {
            var c = new CRC16();

            Assert.Equal(c.ValidateChecksum(items, crc16), isValid);
        }

        [Theory(DisplayName = "Computes a crc16 for a byte array")]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, new byte[] { 0x3D, 0xBB }, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, new byte[] { 0x31, 0x31 }, false)]
        public void CheckCRC16Bytes(byte[] items, byte[] crc16, bool isValid)
        {
            if (crc16 == null) throw new ArgumentNullException(nameof(crc16));
            var c = new CRC16();
            var cal = c.ComputeChecksumBytes(items);
            Assert.Equal((cal[0] == crc16[0]) && (cal[1] == crc16[1]), isValid);
        }

    }
}
