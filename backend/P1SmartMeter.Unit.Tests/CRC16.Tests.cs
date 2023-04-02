
using System.Text;
using Xunit;
using FluentAssertions;

using NLog.Filters;

namespace P1SmartMeter.CRC16Tests
{
    public class ValidateArguments
    {
        [Fact(DisplayName = "ComputeChecksumBytes(byte[] bytes) will NOT throw ArgumentNullException")]
        public void ComputeChecksumBytesNull()
        {
            Action act = () => CRC16.ComputeChecksumBytes(null);
            _ = act.Should().NotThrow<ArgumentNullException>();
        }

        [Fact(DisplayName = "ComputeChecksum(ReadOnlySpan<byte> bytes) will NOT throw ArgumentNullException")]
        public void ComputeChecksumNull1()
        {
            Action act = () => CRC16.ComputeChecksum(null);
            var t = act.Should().NotThrow<ArgumentNullException>();            
        }

        [Fact(DisplayName = "ComputeChecksum(ReadOnlySpan<byte> bytes) will NOT throw ArgumentNullException")]
        public void ComputeChecksumNull2()
        {
            Action act = () => CRC16.ComputeChecksum(null);
            _ = act.Should().NotThrow<ArgumentNullException>();
        }

        [Fact(DisplayName = "ValidateChecksum(byte[] bytes, string checksum) will NOT throw ArgumentNullException")]
        public void ValidateChecksumNull1()
        {
            Action act = () => CRC16.ValidateChecksum(null, "123");
            _ = act.Should().NotThrow<ArgumentNullException>();
        }

        [Fact(DisplayName = "ValidateChecksum(byte[] bytes, string checksum) will throw ArgumentNullException")]
        public void ValidateChecksumNull2()
        {
            Action act = () => CRC16.ValidateChecksum(new byte[] { 0x00 }, null);
            _ = act.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ValidateChecksum(ReadOnlySpan<byte>, ushort checksum) will NOT throw ArgumentNullException")]
        public void ValidateChecksumNull3()
        {
            Action act = () => CRC16.ValidateChecksum(null, 0x1234);
            _ = act.Should().NotThrow<ArgumentNullException>();
        }

        [Fact(DisplayName = "Continue(ReadOnlySpan<byte> bytes) will NOT throw ArgumentNullException")]
        public void ContinueArgumentNullException2()
        {
            CRC16 crc = new();
            Action act = () => crc.Continue(null);
            _ = act.Should().NotThrow<ArgumentNullException>();
        }
    }

    public class ComputationStaticVersion
    {
        [Theory(DisplayName = "Basic compute - validates lookup table")]
        [InlineData(new byte[] { 0x00 }, 0x0000)]
        [InlineData(new byte[] { 0x01 }, 0xC0C1)]
        [InlineData(new byte[] { 0x02 }, 0xC181)]
        [InlineData(new byte[] { 0x03 }, 0x0140)]
        [InlineData(new byte[] { 0x04 }, 0xC301)]
        [InlineData(new byte[] { 0x05 }, 0x03C0)]
        [InlineData(new byte[] { 0x06 }, 0x0280)]
        [InlineData(new byte[] { 0x07 }, 0xC241)]
        [InlineData(new byte[] { 0xF8 }, 0x8201)]
        [InlineData(new byte[] { 0xF9 }, 0x42C0)]
        [InlineData(new byte[] { 0xFA }, 0x4380)]
        [InlineData(new byte[] { 0xFB }, 0x8341)]
        [InlineData(new byte[] { 0xFC }, 0x4100)]
        [InlineData(new byte[] { 0xFD }, 0x81C1)]
        [InlineData(new byte[] { 0xFE }, 0x8081)]
        [InlineData(new byte[] { 0xFF }, 0x4040)]
        public void BasicComputeChecksum(byte[] items, ushort crc16)
        {
            Assert.Equal(CRC16.ComputeChecksum(items), crc16);
        }

        [Theory(DisplayName = "Validates byte array with a given crc16 as a short value")]
        [InlineData(new byte[] { 0x00, 0x00 }, 0x0000, true)]
        [InlineData(new byte[] { 0xAA, 0xBB }, 0xD33E, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0xbb3d, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0x1111, false)]
        public void CheckCRC16Short(byte[] items, ushort crc16, bool isValid)
        {
            Assert.Equal(CRC16.ValidateChecksum(items, crc16), isValid);
        }

        [Theory(DisplayName = "Validates string with a given crc16 as a short value")]
        [InlineData("""123456789""", 0xBB3D, true)]
        [InlineData("""ABCDEFGHIJKLMNOPQRSTUVWXYZ""", 0x18E7, true)]
        [InlineData("""abcdefghijklmnopqrstuvwxyz""", 0x9C1D, true)]
        [InlineData("""hello world""", 0x39C1, true)]
        [InlineData("""a""", 0xe8c1, true)]
        [InlineData(""" """, 0xd801, true)]
        public void CheckCRC16Short2(string str, ushort crc16, bool isValid)
        {
            var items = Encoding.UTF8.GetBytes(str);
            Assert.Equal(CRC16.ValidateChecksum(items, crc16), isValid);
        }

        [Theory(DisplayName = "Validates byte array with a given crc16 as a string value")]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, "BB3D", true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, "1111", false)]
        public void CheckCRC16String(byte[] items, string crc16, bool isValid)
        {
            Assert.Equal(CRC16.ValidateChecksum(items, crc16), isValid);
        }

        [Theory(DisplayName = "Computes a crc16 for a byte array")]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, new byte[] { 0x3D, 0xBB }, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, new byte[] { 0x31, 0x31 }, false)]
        public void CheckCRC16Bytes(byte[] items, byte[] crc16, bool isValid)
        {
            if (crc16 == null) throw new ArgumentNullException(nameof(crc16));
            var cal = CRC16.ComputeChecksumBytes(items);
            Assert.Equal((cal[0] == crc16[0]) && (cal[1] == crc16[1]), isValid);
        }
    }

    public class ComputationObjectVersion
    {
        [Fact]
        public void InitializesProperly()
        {
            CRC16 crc = new();
            crc.CurrentValue.Should().Be(0x0000);
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
        }

        [Fact]
        public void ResetsProperlyToInitial()
        {
            CRC16 crc = new();
            crc.Continue(new byte[] { 0xAA, 0xBB }).Should().Be(0xD33E);
            crc.CurrentValue.Should().NotBe(0x0000);
            crc.CurrentValue.Should().NotBe(CRC16.InitialValue);
            crc.CurrentValue.Should().Be(0xD33E);

            crc.Restart();
            crc.CurrentValue.Should().Be(0x0000);
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
        }

        [Fact]
        public void ContinuesProperly()
        {
            CRC16 crc = new();
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
            crc.Continue(new byte[] { 0xAA, 0xBB }).Should().Be(0xD33E);
            crc.CurrentValue.Should().Be(0xD33E);
            crc.Continue(new byte[] { 0xCC, 0xDD }).Should().Be(0xA4C4);
            crc.CurrentValue.Should().Be(0xA4C4);
        }

        [Fact]
        public void UsesCorrectLength()
        {
            CRC16 crc = new();
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
            crc.Continue(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }).Should().Be(0xA4C4);
            crc.CurrentValue.Should().Be(0xA4C4);
        }

        [Fact]
        public void UsesCorrectLengthWithEmptyArray()
        {
            CRC16 crc = new();
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
            crc.Continue(Array.Empty<byte>()).Should().Be(0x0000);
            crc.CurrentValue.Should().Be(0x0000);
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
        }

        [Fact]
        public void ProperlyIdentifiesNullAsEmptyArray1()
        {
            CRC16 crc = new();
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
            crc.Continue(null).Should().Be(0x0000);
            crc.CurrentValue.Should().Be(0x0000);
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
        }

        [Fact]
        public void ProperlyIdentifiesNullAsEmptyArray2()
        {
            CRC16 crc = new();
            crc.CurrentValue.Should().Be(CRC16.InitialValue);
            crc.Continue(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }).Should().Be(0xA4C4);
            crc.Continue(null).Should().Be(0xA4C4);
            crc.CurrentValue.Should().Be(0xA4C4);
        }
    }
}
