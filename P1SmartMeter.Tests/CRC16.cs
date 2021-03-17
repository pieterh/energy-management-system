using Xunit;

namespace P1SmartMeter.Tests
{
    public class CRC16Tests
    {
        [Theory]
        [InlineData(new byte[] { 0x00 }, 0x0000, true)]
        [InlineData(new byte[] { 0x00, 0x00 }, 0x0000, true)]
        [InlineData(new byte[] { 0xAA, 0xBB }, 0xD33E, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0xbb3d, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, 0x1111, false)]
        public void CheckCRC16_Short(byte[] items, ushort crc16, bool isValid)
        {
            var c = new CRC16();
            
            Assert.Equal(c.ValidateChecksum(items, crc16), isValid);
        }

        [Theory]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, "BB3D", true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, "1111", false)]
        public void CheckCRC16_string(byte[] items, string crc16, bool isValid)
        {
            var c = new CRC16();

            Assert.Equal(c.ValidateChecksum(items, crc16), isValid);
        }

        [Theory]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, new byte[] {  0x3D, 0xBB }, true)]
        [InlineData(new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }, new byte[] { 0x31, 0x31 }, false)]
        public void CheckCRC16_bytes(byte[] items, byte[] crc16, bool isValid)
        {
            var c = new CRC16();
            var cal = c.ComputeChecksumBytes(items);
            Assert.Equal((cal[0] == crc16[0]) && (cal[1] == crc16[1]), isValid);
        }
    }
}
