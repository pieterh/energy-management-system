using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using EMS.Library;

namespace AlfenNG9xx.Tests
{
    public class ConvertersTests
    {
        [Theory]
        [InlineData(new byte[] { }, "")]
        [InlineData(new byte[] { 0x00, 0x00 }, "")]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 }, "")]
        [InlineData(new byte[] { 0x4c, 0x41, 0x2d, 0x46, 0x30, 0x30, 0x30, 0x30, 0x30, 0x33, 0x00, 0x37, 0x00, 0x00, 0x00, 0x00 }, "ALF-0000307")]
        public void ConvertRegisterArrayToStringObject(byte[] bytes, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            string result = Converters.ConvertRegistersToString(ushortArray);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new byte[] { }, 0, 0, "")]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 1, 1, "")]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x6c, 0x41, 0x65, 0x66, 0x20, 0x6e, 0x56, 0x4e, 0x00, 0x00 }, 2, 5, "Alfen NV")]
        public void ConvertRegisterArrayToStringObjectUsingOffset(byte[] bytes, int offset, int nrOfRegisters, string expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];

            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            string result = Converters.ConvertRegistersToString(ushortArray, offset, nrOfRegisters);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new byte[] { 0x00, 0x00 }, 0)]
        [InlineData(new byte[] { 0x01, 0x00, 0x77, 0x77 }, 1)]
        public void ConvertRegisterArrayToShort(byte[] bytes, UInt16 expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersShort(ushortArray);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new byte[] { 0x00, 0x00 }, 0, 0)]
        [InlineData(new byte[] { 0x88, 0x88, 0x01, 0x00, 0x77, 0x77 }, 1, 1)]
        public void ConvertRegisterArrayToShortUsingOffset(byte[] bytes, int offset, UInt16 expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersShort(ushortArray, offset);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertRegisterArrayToShortThrowsExceptionWithIncorrectInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Converters.ConvertRegistersShort(Array.Empty<ushort>()));
        }

        [Theory]
        [InlineData(new byte[] { 0x33, 0x44, 0x11, 0x22 }, 0x44332211)]
        [InlineData(new byte[] { 0x33, 0xFF, 0x11, 0x22 }, 0xFF332211)]
        public void ConvertRegisterArrayToUInt32(byte[] bytes, UInt32 expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersUInt32(ushortArray);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new byte[] { 0x88, 0x88, 0x33, 0x44, 0x11, 0x22 }, 1, 0x44332211)]
        [InlineData(new byte[] { 0x88, 0x88, 0x33, 0x44, 0x11, 0x22, 0x77, 0x77 }, 1, 0x44332211)]
        public void ConvertRegisterArrayToUInt32UsingOffset(byte[] bytes, int offset, UInt32 expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersUInt32(ushortArray, offset);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertRegisterArrayToUInt32ThrowsExceptionWithIncorrectInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Converters.ConvertRegistersUInt32(Array.Empty<ushort>()));
        }

        [Theory]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x00 }, 0x0000000000000011)]
        [InlineData(new byte[] { 0x77, 0x88, 0x55, 0x66, 0x33, 0x44, 0x11, 0x22 }, 0x8877665544332211)]
        public void ConvertRegisterArrayToUInt64(byte[] bytes, UInt64 expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersLong(ushortArray);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new byte[] { 0xAA, 0xAA, 0x77, 0x88, 0x55, 0x66, 0x33, 0x44, 0x11, 0x22 }, 1, 0x8877665544332211)]
        [InlineData(new byte[] { 0xAA, 0xAA, 0x77, 0x88, 0x55, 0x66, 0x33, 0x44, 0x11, 0x22, 0xCC, 0xCC }, 1, 0x8877665544332211)]
        public void ConvertRegisterArrayToUInt64UsingOffset(byte[] bytes, int offset, UInt64 expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersLong(ushortArray, offset);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertRegisterArrayToUInt64ThrowsExceptionWithIncorrectInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Converters.ConvertRegistersLong(Array.Empty<ushort>()));
        }

        [Theory]
        [InlineData(new byte[] { 0x33, 0x44, 0x11, 0x22 }, 716.5323f)]
        [InlineData(new byte[] { 0x33, 0xFF, 0x11, 0x22 }, -2.381087E+38f)]
        [InlineData(new byte[] { 0x40, 0x46, 0x66, 0xe6 }, 12345.6f)]
        public void ConvertRegisterArrayToFloat(byte[] bytes, float expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersFloat(ushortArray);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new byte[] { 0x88, 0x88, 0x33, 0x44, 0x11, 0x22 }, 1, 716.5323f)]
        [InlineData(new byte[] { 0x88, 0x88, 0x33, 0x44, 0x11, 0x22, 0x77, 0x77 }, 1, 716.5323f)]
        public void ConvertRegisterArrayToFloatUsingOffset(byte[] bytes, int offset, float expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersFloat(ushortArray, offset);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertRegisterArrayToFloatThrowsExceptionWithIncorrectInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Converters.ConvertRegistersFloat(Array.Empty<ushort>()));
        }


        [Theory]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0d)]
        [InlineData(new byte[] { 0x77, 0x88, 0x55, 0x66, 0x33, 0x44, 0x11, 0x22 }, -7.086876636573014E-268d)]
        [InlineData(new byte[] { 0x67, 0x41, 0x29, 0x8c, 0xcc, 0xdc, 0xcd, 0xcc }, 12345678.9d)]
        [InlineData(new byte[] { 0x67, 0x41, 0x29, 0x8c, 0xf7, 0xdf, 0xd9, 0xce }, 12345678.999d)]
        [InlineData(new byte[] { 0x67, 0x41, 0x29, 0x8c, 0xf7, 0xdf, 0xee, 0xee }, 12345678.999015298d)]
        public void ConvertRegisterArrayToDouble(byte[] bytes, double expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersDouble(ushortArray);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new byte[] { 0xff, 0xff, 0x77, 0x88, 0x55, 0x66, 0x33, 0x44, 0x11, 0x22 }, 1, -7.086876636573014E-268d)]
        [InlineData(new byte[] { 0xff, 0xff, 0x77, 0x88, 0x55, 0x66, 0x33, 0x44, 0x11, 0x22, 0xff, 0xff }, 1, -7.086876636573014E-268d)]
        public void ConvertRegisterArrayToDoubleUsingOffset(byte[] bytes, int offset, double expectedResult)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ushort[] ushortArray = new ushort[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, ushortArray, 0, bytes.Length);
            var result = Converters.ConvertRegistersDouble(ushortArray, offset);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertRegisterArrayToDoubleThrowsExceptionWithIncorrectInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Converters.ConvertRegistersDouble(Array.Empty<ushort>()));
        }


        [Theory]
        [InlineData(716.5323f, new byte[] { 0x33, 0x44, 0x11, 0x22 })]
        public void ConvertFloatToRegister(float f, byte[] expectedResult)
        {
            ArgumentNullException.ThrowIfNull(expectedResult);
            var result = Converters.ConvertFloatToRegisters(f);

            byte[] bytesArray = new byte[sizeof(ushort) * 2];
            Buffer.BlockCopy(result, 0, bytesArray, 0, bytesArray.Length);

            Assert.Equal(2, result.Length);
            Assert.Equal(expectedResult[0], bytesArray[0]);
            Assert.Equal(expectedResult[1], bytesArray[1]);
            Assert.Equal(expectedResult[2], bytesArray[2]);
            Assert.Equal(expectedResult[3], bytesArray[3]);
        }

        [Theory]
        [InlineData(716.5323f)]
        public void ConvertFloats(float f)
        {
            var result1 = Converters.ConvertFloatToRegisters(f);
            var result2 = Converters.ConvertRegistersFloat(result1);

            Assert.Equal(f, result2);
            Assert.Equal(2, result1.Length);
        }
    }
}
