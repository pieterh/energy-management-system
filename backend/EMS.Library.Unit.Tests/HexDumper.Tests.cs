using System;
using System.Text;
using Xunit;
using EMS.Library;

namespace HexDumperTest
{
    public class HexDumperTest
    {
        [Theory]
        [InlineData(new byte[] { 0x4c, 0x41, 0x2d, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x33, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00 }, "00   4c 41 2d 46 30 30 30 30 30 33 00 30 00 00 00 00   LA-F000003.0....")]
        [InlineData(new byte[] { 0x4c, 0x41, 0x2d, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x33, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x01 }, "00   4c 41 2d 46 30 30 30 30 30 33 00 30 00 00 00 00   LA-F000003.0....\n10   01                                                .")]
        public void HexDumpByteArray(byte[] bytes, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(expectedResult);
            string result = HexDumper.ConvertToHexDump(bytes);
            expectedResult = expectedResult.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
            Assert.Equal(expectedResult, result);
        }
        [Theory]
        [InlineData("", "")]
        [InlineData("To be is to ski.", "00   54 00 6f 00 20 00 62 00 65 00 20 00 69 00 73 00   T.o. .b.e. .i.s.")]
        public void HexDumpString(string inputString, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(expectedResult);
            string result = HexDumper.ConvertToHexDump(inputString);
            expectedResult = expectedResult.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
            Assert.Equal("utf-8", Encoding.Default.WebName);
            Assert.Equal(expectedResult, result);
        }
        [Theory]
        [InlineData(long.MinValue, "0    00 00 00 00 00 00 00 80                           ........")]
        [InlineData(long.MaxValue, "0    ff ff ff ff ff ff ff 7f                           ........")]
        [InlineData(1234567890123456789L, "0    15 81 e9 7d f4 10 22 11                           ...}..\".")]
        public void HexDumpLong(long inputLong, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(expectedResult);
            string result = HexDumper.ConvertToHexDump(inputLong);
            expectedResult = expectedResult.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
            Assert.Equal(expectedResult, result);
        }
        [Theory]
        [InlineData(double.MinValue, "0    ff ff ff ff ff ff ef ff                           ........")]
        [InlineData(double.MaxValue, "0    ff ff ff ff ff ff ef 7f                           ........")]
        [InlineData(1234567890123456789D, "0    81 e9 7d f4 10 22 b1 43                           ..}..\".C")]
        public void HexDumpDouble(double inputDouble, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(expectedResult);
            string result = HexDumper.ConvertToHexDump(inputDouble);
            expectedResult = expectedResult.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
            Assert.Equal(expectedResult, result);
        }
        [Theory]
        [InlineData(UInt64.MinValue, "0    00 00 00 00 00 00 00 00                           ........")]
        [InlineData(UInt64.MaxValue, "0    ff ff ff ff ff ff ff ff                           ........")]
        [InlineData(1234567890123, "0    cb 04 fb 71 1f 01 00 00                           ...q....")]
        public void HexDumpUint64(UInt64 inputUint64, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(expectedResult);
            string result = HexDumper.ConvertToHexDump(inputUint64);
            expectedResult = expectedResult.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
            Assert.Equal(expectedResult, result);
        }
        [Theory]
        [InlineData(float.MinValue, "0    ff ff 7f ff                                       ....")]
        [InlineData(float.MaxValue, "0    ff ff 7f 7f                                       ....")]
        [InlineData(123456789.25F, "0    a3 79 eb 4c                                       .y.L")]
        public void HexDumpFloat(float inputFloat, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(expectedResult);
            string result = HexDumper.ConvertToHexDump(inputFloat);
            expectedResult = expectedResult.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
            Assert.Equal(expectedResult, result);
        }
    }
}