using System;
using Xunit;
using AlfenNG9xx;

namespace AlfenNG9xx.Tests
{
    public class ConvertersTests
    {        
        [Theory]
        [InlineData(new byte[] {}, "")]
        [InlineData(new byte[] {0x00, 0x00}, "")]
        [InlineData(new byte[] {0x00, 0x00, 0x00, 0x00}, "")]
        [InlineData(new byte[] {0x4c, 0x41, 0x2d, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x33, 0x00, 0x37, 0x00, 0x00, 0x00, 0x00}, "ALF-0000307")]
        public void ConvertRegisterArrayToStringObject(byte[] bytes, string expectedResult)
        {
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            string result = Converters.ConvertRegistersToString(ushortArray);
            Assert.True(expectedResult.Equals(result));
        }
        
        [Theory]
        [InlineData(new byte[] {}, 0, 0, "")]
        [InlineData(new byte[] {0x00, 0x00, 0x00, 0x00}, 1, 1, "")]
        [InlineData(new byte[] {0x00, 0x00, 0x00, 0x00, 0x6c, 0x41, 0x65, 0x66, 0x20, 0x6e, 0x56, 0x4e, 0x00, 0x00}, 2, 5, "Alfen NV")]
        public void ConvertRegisterArrayToStringObjectUsingOffset(byte[] bytes, int offset, int nrOfRegisters, string expectedResult)
        {
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            string result = Converters.ConvertRegistersToString(ushortArray, offset, nrOfRegisters);
            Assert.True(expectedResult.Equals(result));
        }    

        [Theory]
        //[InlineData(new byte[] {}, 0)]
        [InlineData(new byte[] {0x00, 0x00}, 0)]
        [InlineData(new byte[] {0x01, 0x00, 0x00, 0x00}, 1)]
        public void ConvertRegisterArrayToShort(byte[] bytes, UInt16 expectedResult)
        {
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            Console.WriteLine(ushortArray.Length);
            var result = Converters.ConvertRegistersShort(ushortArray);
            Console.WriteLine(result);
            Assert.True(expectedResult.Equals(result));
        }     

        [Fact]
        public void ConvertRegisterArrayToShortThrowsExceptionWithIncorrectInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Converters.ConvertRegistersShort(new ushort[0]));
        }         
    }
}
