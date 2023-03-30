using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EMS.Library
{
    public static class HexDumper
    {
        private static readonly CultureInfo culture = CultureInfo.GetCultureInfo("en-US");

        public static string ConvertToHexDump(string str)
        {
            ArgumentNullException.ThrowIfNull(str);
            var bytes = new byte[str.Length];
            Buffer.BlockCopy(str.ToArray(), 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
        public static string ConvertToHexDump(ushort[] shorts)
        {
            ArgumentNullException.ThrowIfNull(shorts);
            var bytes = new byte[shorts.Length * sizeof(short)];
            Buffer.BlockCopy(shorts, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
        public static string ConvertToHexDump(long l)
        {
            long[] larray = { l };
            var bytes = new byte[sizeof(long)];
            Buffer.BlockCopy(larray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
        public static string ConvertToHexDump(double d)
        {
            double[] darray = { d };
            var bytes = new byte[sizeof(double)];
            Buffer.BlockCopy(darray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
        public static string ConvertToHexDump(UInt64 d)
        {
            UInt64[] darray = { d };
            var bytes = new byte[sizeof(double)];
            Buffer.BlockCopy(darray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
        public static string ConvertToHexDump(float l)
        {
            float[] larray = { l };
            var bytes = new byte[sizeof(float)];
            Buffer.BlockCopy(larray, 0, bytes, 0, bytes.Length);
            return ConvertToHexDump(bytes);
        }
        public static string ConvertToHexDump(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            var result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 16)
            {
                if (i > 0) result.AppendLine();
                int padding = bytes.Length.Log16().ExtremeValuesToZero() - i.Log16().ExtremeValuesToZero() + 1;
                byte[] bytesChunk = bytes.Skip(i).Take(16).ToArray();
                result.Append($"{i:X}".PadLeft(padding, '0').PadRight(5, ' '));
                result.Append(ConvertBytesToHex(bytesChunk));
                result.Append(string.Empty.PadRight((17 - bytesChunk.Length) * 3, ' '));
                result.Append(ConvertBytesToString(bytesChunk));
            }

            return result.ToString();
        }

        private static string ConvertBytesToHex(byte[] bytes)
        {
            string hexDump = BitConverter.ToString(bytes);
            hexDump = hexDump.Replace("-", " ", StringComparison.Ordinal);

            return hexDump.ToLower(culture);
        }

        private static string ConvertBytesToString(byte[] bytes)
        {
            var result = new StringBuilder();
            foreach (var hex in bytes)
            {
                char mark = Encoding.ASCII.GetChars(new byte[] { hex })[0];
                if (char.IsControl(mark) || char.IsSurrogate(mark) || mark.Equals('?'))
                    result.Append('.');
                else
                    result.Append(mark);
            }

            return result.ToString();
        }

        internal static int Log16(this int input)
        {
            return (int)System.Math.Floor(System.Math.Log(input, 16));
        }

        internal static int ExtremeValuesToZero(this int input)
        {
            if (input == int.MinValue)
                return 0;
            return input;
        }
    }
}