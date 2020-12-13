using System;
using System.Text;
using Xunit;

namespace EMS.Library.Tests
{
    public class HexDumperTests
    {
        [Fact]
        public void HexDumpByteArray(byte[] bytes, string expectedResult)
        {
            string result = HexDumper.ConvertToHexDump(bytes);
            expectedResult = expectedResult.Replace("\n", Environment.NewLine);
            Assert.Equal(expectedResult, result);
        }

    }
}